-- s16556.dbo.RefreshToken definition

-- Drop table

-- DROP TABLE s16556.dbo.RefreshToken GO

CREATE TABLE s16556.dbo.RefreshToken (
	Id nvarchar(36) COLLATE Polish_CI_AS NOT NULL,
	IndexNumber nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	CONSTRAINT RefreshToken_pk PRIMARY KEY (Id)
) GO;


-- s16556.dbo.RefreshToken foreign keys

ALTER TABLE s16556.dbo.RefreshToken ADD CONSTRAINT RefreshToken_Student FOREIGN KEY (IndexNumber) REFERENCES s16556.dbo.Student(IndexNumber) GO;