-- Script para criar banco de dados SimpleConnect no PostgreSQL
-- Execute este script como usuário postgres no psql

-- =================================================================
-- CONFIGURAÇÕES DE AMBIENTE
-- =================================================================

-- Ambiente de Desenvolvimento Local
-- POSTGRESDB_HOST: 192.168.68.113
-- POSTGRESDB_PORT: 5432
-- REDIS_HOST: 192.168.68.113
-- REDIS_PORT: 6379

-- Ambiente Docker/Container
-- POSTGRESDB_HOST: host.docker.internal
-- POSTGRESDB_PORT: 5432
-- REDIS_HOST: host.docker.internal
-- REDIS_PORT: 6379

-- =================================================================
-- POSTGRESQL SETUP
-- =================================================================

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

-- 6. Criar índices otimizados para performance
-- Estes índices serão criados pela aplicação, mas podemos criar aqui para garantir
CREATE INDEX IF NOT EXISTS idx_chat_messages_group_created 
ON "ChatMessages"("ChatGroupId", "CreatedAt" DESC);

CREATE INDEX IF NOT EXISTS idx_chat_groups_creator 
ON "ChatGroups"("CreatorId");

CREATE INDEX IF NOT EXISTS idx_chat_group_members_user 
ON "ChatGroupMembers"("UserId");

CREATE INDEX IF NOT EXISTS idx_chat_group_members_group 
ON "ChatGroupMembers"("ChatGroupId");

-- 7. Criar funções para otimização de consultas
CREATE OR REPLACE FUNCTION update_chat_group_member_count()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "ChatGroups" 
    SET "CurrentParticipantsCount" = (
        SELECT COUNT(*) FROM "ChatGroupMembers" 
        WHERE "ChatGroupId" = NEW."ChatGroupId" AND "IsActive" = true
    )
    WHERE "Id" = NEW."ChatGroupId";
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 8. Criar triggers para manter contadores atualizados
CREATE TRIGGER trigger_update_chat_group_member_count
    AFTER INSERT OR DELETE OR UPDATE ON "ChatGroupMembers"
    FOR EACH ROW EXECUTE FUNCTION update_chat_group_member_count();

-- 9. Mostrar informações do banco de dados
\l simpleconnect;

-- =================================================================
-- REDIS CONFIGURAÇÃO (separada - execute no Redis CLI)
-- =================================================================

-- Comandos para configurar Redis (execute no redis-cli):
-- redis-cli -h 192.168.68.113 -p 6379

-- Configurações recomendadas para desenvolvimento:
-- CONFIG SET maxmemory 256mb
-- CONFIG SET maxmemory-policy allkeys-lru
-- CONFIG SET save 900 1
-- CONFIG SET save 300 10
-- CONFIG SET save 60 10000

-- Para testar conexão:
-- PING
-- SET test_key "Hello Redis!"
-- GET test_key

-- =================================================================
-- NOTAS IMPORTANTES
-- =================================================================

-- PostgreSQL:
-- - A aplicação irá criar automaticamente as tabelas quando iniciar (Code-First)
-- - Os dados iniciais (seed) serão inseridos automaticamente
-- - Para desenvolvimento local: use postgres/192.168.68.113:5432
-- - Para Docker: use host.docker.internal:5432
-- - Em produção, crie usuário dedicado com senha forte

-- Redis:
-- - Usado para cache de sessões, mensagens e estado de chamadas
-- - Porta padrão: 6379
-- - Para desenvolvimento local: 192.168.68.113:6379
-- - Para Docker: host.docker.internal:6379
-- - Configure maxmemory-policy para evitar estouro de memória

-- Performance:
-- - Índices compostos em ChatGroupId + CreatedAt para paginação eficiente
-- - Cache Redis para dados frequentemente acessados (grupos, usuários online)
-- - Connection pooling configurado no application settings
-- - Soft delete implementado para histórico de mensagens

-- Segurança:
-- - Use SSL/TLS em produção
-- - Configure firewall para permitir apenas IPs necessários
-- - Use senhas fortes para usuários de banco
-- - Configure Redis com autenticação (requirepass)
