# Plan de RecuperaciÃ³n ante Desastres - AfterQuake

## Escenarios

### 1. Falla de base de datos
1. Detener aplicaciÃ³n
2. Restaurar Ãºltimo backup completo:
   ```sql
   RESTORE DATABASE [AfterQuake] FROM DISK = 'C:\Backups\AfterQuake\AfterQuake_Full_*.bak' WITH RECOVERY
   ```
3. Aplicar Ãºltimos logs de transacciÃ³n
4. Reanudar aplicaciÃ³n

### 2. Falla de servidor completo
1. Provisionar nuevo servidor
2. Ejecutar docker-compose up:
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```
3. Restaurar BD desde backup en blob storage
4. Verificar health checks en /health/ready

### 3. Falla de Redis
- SignalR backplane se degrada a single-instance
- Cache distribuido deja de funcionar
- La aplicaciÃ³n sigue operativa sin Redis
- Plan: Reiniciar servicio Redis, luego reiniciar app

## RPO y RTO
- RPO (Recovery Point Objective): 6 horas (backups cada 6h + log shipping)
- RTO (Recovery Time Objective): 2 horas para restauraciÃ³n completa

## Contactos
- DBA: dba@afterquake.cl
- DevOps: devops@afterquake.cl
- Seguridad: security@afterquake.cl
