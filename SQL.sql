CREATE DATABASE ptachiya;
GO

USE ptachiya;
GO

CREATE TABLE Kindergarten (
    KindergartenId INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(100) NOT NULL
);


CREATE TABLE Payment (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    ChildId INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    PaymentDate DATETIME NULL
);

CREATE TABLE Child (
    ChildId INT IDENTITY(1,1) PRIMARY KEY,
    KindergartenId INT NOT NULL,
    IdNumber NVARCHAR(20) NOT NULL,
    BirthDate DATE NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    SchoolYear NVARCHAR(20) NOT NULL,
    FormLink NVARCHAR(200) NULL,
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PaymentId INT NULL,
    CONSTRAINT FK_Child_Kindergarten FOREIGN KEY (KindergartenId)
        REFERENCES Kindergarten(KindergartenId),
    CONSTRAINT FK_Child_Payment FOREIGN KEY (PaymentId)
        REFERENCES Payment(PaymentId)
);

ALTER TABLE Payment
ADD CONSTRAINT FK_Payment_Child FOREIGN KEY (ChildId)
REFERENCES Child(ChildId);


CREATE TABLE Form (
    FormId INT IDENTITY(1,1) PRIMARY KEY,
    ChildId INT NOT NULL,
    FormLink NVARCHAR(200) NOT NULL,
    SubmittedDate DATETIME NULL,
    CONSTRAINT FK_Form_Child FOREIGN KEY (ChildId)
        REFERENCES Child(ChildId)
);


CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL
);

SELECT * FROM Kindergartens;
GO

SELECT * FROM Children;
GO

SELECT * FROM Payments;
GO

SELECT * FROM Forms;
GO

SELECT * FROM [Users];
GO


INSERT INTO [Users] (Username, PasswordHash) VALUES ('michal', '214356271');