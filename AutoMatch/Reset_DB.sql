-- Desativar constraints temporariamente
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- Apagar dados
EXEC sp_MSforeachtable 'DELETE FROM ?';

-- Resetar identity
EXEC sp_MSforeachtable '
IF OBJECTPROPERTY(OBJECT_ID(''?''), ''TableHasIdentity'') = 1
    DBCC CHECKIDENT (''?'', RESEED, 0)
';

-- Reativar constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';