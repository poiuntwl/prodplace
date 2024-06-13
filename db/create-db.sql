CREATE
    DATABASE ProdPlace
GO

USE ProdPlace;
GO

CREATE TABLE Products
(
    Id          int            not null identity,
    Name        nvarchar(200)  not null,
    Description nvarchar(max)  not null,
    Price       decimal(19, 4) not null,
    PRIMARY KEY (Id)
);
GO

INSERT INTO Products (Name, Description, Price)
VALUES ('T-Shirt Blue', 'Its blue', 17.00),
       ('T-Shirt Black', 'Its black', 16.99);
GO
