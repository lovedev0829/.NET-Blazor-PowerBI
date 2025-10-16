-- Complete script to create AssessmentData table and seed sample data
USE sms_db;
GO

-- Drop table if exists (for development purposes)
IF EXISTS (SELECT * FROM sysobjects WHERE name='AssessmentData' AND xtype='U')
BEGIN
    DROP TABLE AssessmentData;
END
GO

-- Create AssessmentData table
CREATE TABLE AssessmentData (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AssessmentType NVARCHAR(50) NOT NULL, -- SBA, NGSS
    Subject NVARCHAR(50) NOT NULL, -- ELA, Math, Science
    Grade INT NOT NULL,
    AcademicYear NVARCHAR(20) NOT NULL, -- 2018-19, 2023-24, 2024-25
    DataCategory NVARCHAR(100) NOT NULL, -- Meeting or Exceeding Goal, High Needs
    Region NVARCHAR(50) NOT NULL, -- Darien, DRG Avg, State Avg
    Percentage DECIMAL(5,2) NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL
);

-- Create indexes for better performance
CREATE INDEX IX_AssessmentData_AssessmentType_Subject_Grade ON AssessmentData (AssessmentType, Subject, Grade);
CREATE INDEX IX_AssessmentData_AcademicYear ON AssessmentData (AcademicYear);
CREATE INDEX IX_AssessmentData_DataCategory ON AssessmentData (DataCategory);
CREATE INDEX IX_AssessmentData_Region ON AssessmentData (Region);
GO

-- Insert sample data for SBA ELA Grade 3 - Meeting or Exceeding Goal
INSERT INTO AssessmentData (AssessmentType, Subject, Grade, AcademicYear, DataCategory, Region, Percentage, CreatedDate) VALUES
('SBA', 'ELA', 3, '2018-19', 'Meeting or Exceeding Goal', 'Darien', 76.00, GETDATE()),
('SBA', 'ELA', 3, '2018-19', 'Meeting or Exceeding Goal', 'DRG Avg', 75.00, GETDATE()),
('SBA', 'ELA', 3, '2018-19', 'Meeting or Exceeding Goal', 'State Avg', 46.00, GETDATE()),
('SBA', 'ELA', 3, '2023-24', 'Meeting or Exceeding Goal', 'Darien', 79.00, GETDATE()),
('SBA', 'ELA', 3, '2023-24', 'Meeting or Exceeding Goal', 'DRG Avg', 76.00, GETDATE()),
('SBA', 'ELA', 3, '2023-24', 'Meeting or Exceeding Goal', 'State Avg', 46.00, GETDATE()),
('SBA', 'ELA', 3, '2024-25', 'Meeting or Exceeding Goal', 'Darien', 82.00, GETDATE()),
('SBA', 'ELA', 3, '2024-25', 'Meeting or Exceeding Goal', 'DRG Avg', 78.00, GETDATE()),
('SBA', 'ELA', 3, '2024-25', 'Meeting or Exceeding Goal', 'State Avg', 47.00, GETDATE());

-- Insert sample data for SBA ELA Grade 3 - High Needs
INSERT INTO AssessmentData (AssessmentType, Subject, Grade, AcademicYear, DataCategory, Region, Percentage, CreatedDate) VALUES
('SBA', 'ELA', 3, '2018-19', 'High Needs', 'Darien', 45.00, GETDATE()),
('SBA', 'ELA', 3, '2023-24', 'High Needs', 'Darien', 44.00, GETDATE()),
('SBA', 'ELA', 3, '2024-25', 'High Needs', 'Darien', 60.00, GETDATE());

-- Insert sample data for SBA Math Grade 3 - Meeting or Exceeding Goal
INSERT INTO AssessmentData (AssessmentType, Subject, Grade, AcademicYear, DataCategory, Region, Percentage, CreatedDate) VALUES
('SBA', 'Math', 3, '2018-19', 'Meeting or Exceeding Goal', 'Darien', 81.00, GETDATE()),
('SBA', 'Math', 3, '2018-19', 'Meeting or Exceeding Goal', 'DRG Avg', 79.00, GETDATE()),
('SBA', 'Math', 3, '2018-19', 'Meeting or Exceeding Goal', 'State Avg', 50.00, GETDATE()),
('SBA', 'Math', 3, '2023-24', 'Meeting or Exceeding Goal', 'Darien', 88.00, GETDATE()),
('SBA', 'Math', 3, '2023-24', 'Meeting or Exceeding Goal', 'DRG Avg', 82.00, GETDATE()),
('SBA', 'Math', 3, '2023-24', 'Meeting or Exceeding Goal', 'State Avg', 52.00, GETDATE()),
('SBA', 'Math', 3, '2024-25', 'Meeting or Exceeding Goal', 'Darien', 88.00, GETDATE()),
('SBA', 'Math', 3, '2024-25', 'Meeting or Exceeding Goal', 'DRG Avg', 84.00, GETDATE()),
('SBA', 'Math', 3, '2024-25', 'Meeting or Exceeding Goal', 'State Avg', 52.00, GETDATE());

-- Insert sample data for SBA Math Grade 3 - High Needs
INSERT INTO AssessmentData (AssessmentType, Subject, Grade, AcademicYear, DataCategory, Region, Percentage, CreatedDate) VALUES
('SBA', 'Math', 3, '2018-19', 'High Needs', 'Darien', 0.00, GETDATE()),
('SBA', 'Math', 3, '2023-24', 'High Needs', 'Darien', 70.00, GETDATE()),
('SBA', 'Math', 3, '2024-25', 'High Needs', 'Darien', 65.00, GETDATE());

PRINT 'AssessmentData table created and seeded successfully!';
GO
