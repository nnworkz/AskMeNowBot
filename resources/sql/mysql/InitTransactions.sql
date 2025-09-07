CREATE TABLE IF NOT EXISTS transactions
(
    id         BIGINT PRIMARY KEY AUTO_INCREMENT,
    user_id    BIGINT       NOT NULL,
    type       VARCHAR(16)  NOT NULL,
    amount     BIGINT       NOT NULL,
    currency   VARCHAR(16)  NOT NULL,
    created_at TIMESTAMP(0) NOT NULL,
    INDEX idx_user_id (user_id)
);
