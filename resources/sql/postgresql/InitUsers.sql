CREATE TABLE IF NOT EXISTS users
(
    id              BIGINT PRIMARY KEY,
    language        CHAR(2)                     NOT NULL,
    role            VARCHAR(16)                 NOT NULL,
    messages        INT                         NOT NULL,
    messages_today  INT                         NOT NULL,
    last_reset_at   TIMESTAMP(0) WITH TIME ZONE NOT NULL,
    last_message_at TIMESTAMP(0) WITH TIME ZONE,
    registered_at   TIMESTAMP(0) WITH TIME ZONE NOT NULL
)
