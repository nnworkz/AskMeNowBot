INSERT INTO filters (user_id, spam, terrorism, drugs, violence, pornography)
VALUES (@user_id, @spam, @terrorism, @drugs, @violence, @pornography)
ON DUPLICATE KEY UPDATE spam        = @spam,
                        terrorism   = @terrorism,
                        drugs       = @drugs,
                        violence    = @violence,
                        pornography = @pornography
