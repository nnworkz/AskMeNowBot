DO
$$
    BEGIN
        CREATE TABLE IF NOT EXISTS links
        (
            link           VARCHAR(12) PRIMARY KEY,
            user_id        BIGINT  NOT NULL,
            used           BIGINT  NOT NULL,
            max_usages     BIGINT,
            is_deactivated BOOLEAN NOT NULL,
            expires_at     TIMESTAMP(0) WITH TIME ZONE
        );

        CREATE INDEX IF NOT EXISTS idx_user_id ON links (user_id);
    END
$$;
