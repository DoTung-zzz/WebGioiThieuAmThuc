-- Add AvatarUrl column to Users table
ALTER TABLE Users
ADD avatar_url NVARCHAR(255) NULL;
