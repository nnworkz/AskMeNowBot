INSERT INTO filters (user_id, spam, terrorism, drugs, violence, pornography)
VALUES (@user_id, @spam, @terrorism, @drugs, @violence, @pornography)
ON CONFLICT (user_id) DO UPDATE SET spam        = @spam,
                                    terrorism   = @terrorism,
                                    drugs       = @drugs,
                                    violence    = @violence,
                                    pornography = @pornography
