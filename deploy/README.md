# Deploy - AfterQuake

## Requisitos de ProducciÃ³n
- SQL Server 2022 Standard o superior (con Availability Groups recomendado)
- Redis 7 Enterprise (redundante)
- .NET 8 Runtime
- Certificado SSL vÃ¡lido
- Azure App Service Plan: P2v3 (3 instancias mÃ­nimo)
- Azure SQL: S2 o superior (geo-replication recomendado)

## Monitoreo
- Application Insights para telemetrÃ­a
- Serilog a Application Insights + archivo
- Health checks cada 30s desde el balanceador
- Alertas en: DB caÃ­da >5min, CPU >80%, 5xx rate >1%

## Escalamiento
- Horizontal: 3-10 instancias segÃºn demanda
- Auto-scale: CPU >70% durante 10min â†’ +1 instancia
- MÃ¡ximo: 20 instancias
