CREATE TABLE [dbo].[customer] (
    [Id]           INT          IDENTITY (1, 1) NOT NULL,
    [name]     VARCHAR (50)  NULL,
    [email]    VARCHAR (50)  NULL,
    [job]      VARCHAR (50)  NULL,
    [married]  BIT           NULL,
    [gender]   VARCHAR (50)  NULL,
    [location] VARCHAR (MAX) DEFAULT (' ') NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

