CREATE TABLE IF NOT EXISTS users
(
    user_id         BIGINT PRIMARY KEY,
    language        CHAR(2)      NOT NULL,
    role            VARCHAR(16)  NOT NULL,
    messages        INT          NOT NULL,
    messages_today  INT          NOT NULL,
    last_reset_at   TIMESTAMP(0) NOT NULL,
    last_message_at TIMESTAMP(0),
    registered_at   TIMESTAMP(0) NOT NULL
)
