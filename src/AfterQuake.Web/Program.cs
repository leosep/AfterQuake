using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using AfterQuake.Application.Interfaces;
using AfterQuake.Domain.Interfaces;
using AfterQuake.Infrastructure.Data;
using AfterQuake.Infrastructure.Services;
using AfterQuake.Web.Hubs;
using AfterQuake.Web.Middleware;
using AfterQuake.Web.HealthChecks;
using AfterQuake.Web.Services;
using Serilog;
using Serilog.Sinks.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using FluentValidation;
using FluentValidation.AspNetCore;
using StackExchange.Redis;

namespace AfterQuake.Web;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/afterquake-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
            .WriteTo.ApplicationInsights(
                TelemetryConfiguration.CreateDefault(),
                TelemetryConverter.Traces)
            .Enrich.FromLogContext()
            .CreateLogger();

        try
        {
            Log.Information("Starting AfterQuake application");

            var builder = WebApplication.CreateBuilder(args);

            var aiConnStr = builder.Configuration["ApplicationInsights:ConnectionString"];
            if (!string.IsNullOrEmpty(aiConnStr))
            {
                builder.Services.AddApplicationInsightsTelemetry(options =>
                {
                    options.ConnectionString = aiConnStr;
                });
            }

            builder.Host.UseSerilog();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
            var redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled");
            var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(5);
                    sqlOptions.CommandTimeout(30);
                }));

            // Identity
            builder.Services.AddIdentity<AfterQuake.Domain.Entities.ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 12;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
                options.Tokens.AuthenticatorTokenProvider = "Authenticator";
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Distributed Cache (Redis or in-memory fallback)
            if (redisEnabled)
            {
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "AfterQuake:";
                });
            }
            else
            {
                builder.Services.AddDistributedMemoryCache();
            }

            // DI - Domain/Application/Infrastructure services
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IEmergencyService, EmergencyService>();
            builder.Services.AddScoped<IPersonReportService, PersonReportService>();
            builder.Services.AddScoped<IHelpRequestService, HelpRequestService>();
            builder.Services.AddScoped<IShelterService, ShelterService>();
            builder.Services.AddScoped<IAlertService, AlertService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddScoped<HaversineService>();
    builder.Services.AddScoped<IGeoService>(sp => sp.GetRequiredService<HaversineService>());
    builder.Services.AddScoped<IHaversineService>(sp => sp.GetRequiredService<HaversineService>());
            builder.Services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
            {
                client.BaseAddress = new Uri("https://nominatim.openstreetmap.org");
                client.DefaultRequestHeaders.Add("User-Agent", "AfterQuake/1.0");
            });
            builder.Services.AddHostedService<BackgroundJobsService>();
            builder.Services.AddHostedService<DatabaseSeedService>();
            builder.Services.AddTransient<JwtService>();
            builder.Services.AddSingleton<CaptchaService>();
            builder.Services.AddSingleton<MetricsService>();
            builder.Services.AddTransient<EmailService>();
            builder.Services.AddScoped<ExportService>();
            builder.Services.AddScoped<SlaService>();

            // JWT Configuration
            var jwtKey = !string.IsNullOrEmpty(builder.Configuration["Jwt:Key"]) ? builder.Configuration["Jwt:Key"]! : "AfterQuake_SuperSecret_Key_2024_Must_Be_32_Chars!";
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AfterQuake";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AfterQuakeAPI";

            builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken))
                                context.Token = accessToken;
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Administrator", "SuperAdministrator"));
                options.AddPolicy("RequireSuperAdmin", policy => policy.RequireRole("SuperAdministrator"));
                options.AddPolicy("RequireVerified", policy => policy.RequireRole("Citizen", "Volunteer", "ReliefOrganization", "Administrator", "SuperAdministrator"));
            });

            // MVC + FluentValidation
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<AfterQuake.Web.Filters.SecurityHeadersFilter>();
            });

            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<AfterQuake.Application.Validators.CreateEmergencyReportValidator>();

            // API Controllers (already registered via AddControllersWithViews)
            builder.Services.AddControllers()
                .AddApplicationPart(typeof(Program).Assembly)
                .AddControllersAsServices();

            // SignalR (with Redis backplane for scale-out if enabled)
            var signalR = builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = builder.Environment.IsDevelopment();
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.MaximumReceiveMessageSize = 128 * 1024;
            });

            if (redisEnabled)
            {
                signalR.AddStackExchangeRedis(redisConnectionString, options =>
                {
                    options.Configuration.ChannelPrefix = RedisChannel.Literal("AfterQuake");
                });
            }

            // Health Checks
            var healthChecks = builder.Services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>(name: "database", tags: new[] { "ready" })
                .AddCheck<AfterQuakeHealthCheck>("afterquake_seed", tags: new[] { "ready" });

            if (redisEnabled)
            {
                healthChecks.AddRedis(redisConnectionString, name: "redis", tags: new[] { "ready" });
            }

            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            builder.Services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
            });

            // Rate Limiting
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;

                options.AddPolicy("ApiRateLimit", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                options.AddPolicy("StrictRateLimit", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            builder.Services.AddHttpClient();
            builder.Services.AddMemoryCache();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("EmergencyApp", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "AfterQuake API",
                    Version = "v1",
                    Description = "API de Respuesta y Coordinación ante Terremotos"
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Ingresa tu token JWT"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/error/500");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();
            app.UseHttpsRedirection();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AfterQuake API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            // PWA - Serve static files with long cache
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (ctx.File.Name.Contains("service-worker"))
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
                    }
                    else
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
                    }
                }
            });

            app.UseRouting();

            // Custom middleware
            app.UseCors("EmergencyApp");
            app.UseIpBlocking();
            app.UseRateLimiter();
            app.UseLocalization();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UsePasswordExpiration();

            // Health check endpoint
            app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var json = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            duration = e.Value.Duration.TotalMilliseconds
                        }),
                        duration = report.TotalDuration.TotalMilliseconds
                    });
                    await context.Response.WriteAsync(json);
                }
            });

            app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = _ => false
            });

            // Routes
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Admin}/{action=Dashboard}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapGet("/metrics", (MetricsService metrics) => Results.Content(metrics.PrometheusFormat(), "text/plain"));

            app.MapPost("/csp-report", async (HttpContext context) =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var report = await reader.ReadToEndAsync();
                Serilog.Log.Warning("CSP Violation: {Report}", report);
                return Results.Ok();
            });

            app.MapHub<NotificationHub>("/hubs/notifications");
            app.MapHub<EmergencyHub>("/hubs/emergency");

            Log.Information("AfterQuake started successfully");
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
