DO
$$
    BEGIN
        CREATE TABLE IF NOT EXISTS transactions
        (
            id         BIGSERIAL PRIMARY KEY,
            user_id    BIGINT                      NOT NULL,
            type       VARCHAR(32)                 NOT NULL,
            amount     BIGINT                      NOT NULL,
            currency   VARCHAR(16)                 NOT NULL,
            created_at TIMESTAMP(0) WITH TIME ZONE NOT NULL
        );

        CREATE INDEX IF NOT EXISTS idx_user_id ON transactions (user_id);
    END
$$;
