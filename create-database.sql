-- Script para criar banco de dados FamilyMeet no PostgreSQL
-- Execute este script como usuário postgres no psql

-- 1. Criar o banco de dados
CREATE DATABASE FamilyMeet
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- 2. Habilitar extensões necessárias
\c FamilyMeet;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 3. Mostrar informações do banco de dados
\l FamilyMeet;

-- 4. Verificar se as tabelas serão criadas pela aplicação
-- As tabelas abaixo serão criadas automaticamente pelo EF Core:
-- - AppChatChatGroups
-- - AppChatChatMessages  
-- - AppChatChatParticipants
-- - AbpUsers (Identity)
-- - AbpRoles (Identity)
-- - AbpTenants (Tenant Management)
-- - E outras tabelas do ABP Framework

-- 5. Para verificar as tabelas após a migration:
-- \dt

-- 6. Para verificar a estrutura de uma tabela específica:
-- \d AppChatChatGroups

-- Nota: Execute o comando abaixo após criar o banco:
-- cd src/api/src/afonsoft.FamilyMeet.DbMigrator && dotnet run
