USE [hesgcDB]
GO

/****** Objet : Table [dbo].[Message] Date du script : 30.07.2018 11:40:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Message] (
    [Message_id]       INT            IDENTITY (1, 1) NOT NULL,
    [Device_idAzure]   VARCHAR (500)  NOT NULL,
    [Protocol_version] SMALLINT       NOT NULL,
    [Message_data]     VARCHAR (5000) NOT NULL,
    [Sending_date]     DATETIME2 (7)  NOT NULL,
    [Created_date]     DATETIME2 (7)  NOT NULL
);


