INSERT INTO transactions (user_id, type, amount, currency, created_at)
VALUES (@user_id, LOWER(@type), @amount, LOWER(@currency), @created_at)
ON DUPLICATE KEY UPDATE user_id    = @user_id,
                        type       = LOWER(@type),
                        amount     = @amount,
                        currency   = LOWER(@currency),
                        created_at = @created_at
