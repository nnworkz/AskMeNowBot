INSERT INTO users (id, language, role, messages, messages_today, last_reset_at, last_message_at, registered_at)
VALUES (@id, LOWER(@language), LOWER(@role), @messages, @messages_today, @last_reset_at, @last_message_at,
        @registered_at)
ON CONFLICT (id) DO UPDATE SET language        = LOWER(@language),
                               role            = LOWER(@role),
                               messages        = @messages,
                               messages_today  = @messages_today,
                               last_reset_at   = @last_reset_at,
                               last_message_at = @last_message_at,
                               registered_at   = @registered_at
