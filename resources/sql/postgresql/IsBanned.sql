SELECT EXISTS (SELECT 1 FROM bans WHERE sender_id = @sender_id AND recipient_id = @recipient_id)
