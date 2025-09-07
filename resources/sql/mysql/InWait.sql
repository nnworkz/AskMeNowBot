SELECT EXISTS (SELECT 1 FROM waits WHERE sender_id = @sender_id)
