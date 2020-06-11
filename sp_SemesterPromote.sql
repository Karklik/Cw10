CREATE PROCEDURE sp_SemesterPromote(
    @id_study AS INT
    ,@semester AS INT
)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;
    -- Get current enrollment
    DECLARE @current_id_enrollment INT=(
        SELECT IdEnrollment
        FROM Enrollment
        WHERE IdStudy = @id_study AND Semester = @semester
    );
    -- If current enrollment dosen't exists stop procedure
    IF @current_id_enrollment IS NULL
    BEGIN
        ROLLBACK;
        RETURN;
    END;
    -- Get enrollment for next semester
    DECLARE @next_semester INT = (@semester + 1)
    DECLARE @next_id_enrollment INT = (
        SELECT IdEnrollment
        FROM Enrollment
        WHERE IdStudy = @id_study AND Semester = @next_semester
    );
    -- If enrollment for next semester dosen't exists create it
    IF @next_id_enrollment IS NULL
    BEGIN
        -- Get next IdEnrollment for new entry
        SET @next_id_enrollment = (
            SELECT MAX(IdEnrollment)
            FROM Enrollment
        ) + 1;
        -- Crates new enrollment for studies
        INSERT INTO 
            Enrollment
        VALUES(
            @next_id_enrollment, 
            @semester + 1,
            @id_study,
            GETDATE());
    END;
    -- Update students with current enrollment to new enrollment
    UPDATE
        Student
    SET
        IdEnrollment = @next_id_enrollment
    WHERE
        IdEnrollment = @current_id_enrollment
  	SELECT * FROM Enrollment WHERE IdEnrollment = @next_id_enrollment
    COMMIT;
END;