-- Script para criar banco de dados SimpleConnect no PostgreSQL
-- Execute este script como usuário postgres no psql

-- 1. Criar o banco de dados
CREATE DATABASE simpleconnect
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- 2. Criar usuário para a aplicação (opcional, se não usar o postgres)
CREATE USER simpleconnect_user WITH PASSWORD 'simpleconnect_password';

-- 3. Conceder permissões ao usuário
GRANT ALL PRIVILEGES ON DATABASE simpleconnect TO simpleconnect_user;

-- 4. Conceder permissões no schema public
\c simpleconnect;
GRANT ALL ON SCHEMA public TO simpleconnect_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO simpleconnect_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO simpleconnect_user;

-- 5. Habilitar extensões necessárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 6. Mostrar informações do banco de dados
\l simpleconnect

-- Notas:
-- - A aplicação irá criar automaticamente as tabelas quando iniciar
-- - Os dados iniciais (seed) serão inseridos automaticamente
-- - Para testar localmente, você pode usar o usuário postgres com a senha "postgres"
-- - Em produção, crie um usuário dedicado com senha forte
