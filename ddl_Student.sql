-- s16556.dbo.Student definition

-- Drop table

-- DROP TABLE s16556.dbo.Student GO

CREATE TABLE s16556.dbo.Student (
	IndexNumber nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	FirstName nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	LastName nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	BirthDate date NOT NULL,
	IdEnrollment int NOT NULL,
	Password nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	Salt nvarchar(100) COLLATE Polish_CI_AS NOT NULL,
	CONSTRAINT Student_pk PRIMARY KEY (IndexNumber)
) GO;


-- s16556.dbo.Student foreign keys

ALTER TABLE s16556.dbo.Student ADD CONSTRAINT Student_Enrollment FOREIGN KEY (IdEnrollment) REFERENCES s16556.dbo.Enrollment(IdEnrollment) GO;