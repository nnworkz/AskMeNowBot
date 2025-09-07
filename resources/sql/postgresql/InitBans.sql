CREATE TABLE IF NOT EXISTS bans
(
    sender_id    BIGINT                      NOT NULL,
    recipient_id BIGINT                      NOT NULL,
    reason       VARCHAR(4000),
    banned_at    TIMESTAMP(0) WITH TIME ZONE NOT NULL,
    PRIMARY KEY (sender_id, recipient_id)
)
