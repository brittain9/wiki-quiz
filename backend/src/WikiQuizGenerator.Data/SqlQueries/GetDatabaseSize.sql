SELECT 
    table_name, 
    pg_size_pretty(pg_total_relation_size('"' || table_schema || '"."' || table_name || '"')) AS total_size
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY pg_total_relation_size('"' || table_schema || '"."' || table_name || '"') DESC;
