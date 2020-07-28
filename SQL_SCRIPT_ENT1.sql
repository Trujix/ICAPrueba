-- HACEMOS USO DE LA BASE DE DATOS DE ICA
USE [ICA];

-- CREACION DE LA TABLA REACCIONTORQUEVACIO
CREATE TABLE [dbo].[RangoTorqueVacio](
	[RTVId] [int] IDENTITY(1,1) NOT NULL,
	[Base] [varchar](10) NOT NULL,
	[ValorTorque] [float] NOT NULL,
	[TamGrano] [float] NOT NULL,
	[Resultado] [float] NOT NULL,
	[Estatus] [varchar](10) NOT NULL,
	[MFechaHora] [datetime] NOT NULL,
	[MUsuarioId] [varchar](50) NOT NULL,
		CONSTRAINT [PK_RangoTorqueVacio] PRIMARY KEY CLUSTERED ([RTVId] ASC)
);

-- LIMPIEZA DE TABLA ANTIGUA DE RANGOS CRITERIOS
DROP TABLE IF EXISTS dbo.RangoCriterio;

-- CREACION DE LA NUEVA VERSION DE TABLA RANGOSCRITERIOS
CREATE TABLE [dbo].[RangoCriterio](
	[RNGId] [int] IDENTITY(1,1) NOT NULL,
	[CRTId ] [int] NOT NULL,
	[Base] [varchar](10) NOT NULL,
	[ValorMin] [float] NOT NULL DEFAULT 0,
	[ValorMax] [float] NOT NULL DEFAULT 0,
	[MFechaHora] [datetime] NOT NULL,
	[MUsuarioId] [varchar](50) NOT NULL,
		CONSTRAINT [PK_RangoCriterio] PRIMARY KEY CLUSTERED ([RNGId] ASC)
);

-- INSERCION DE VALOR DE PRUEBA
INSERT [dbo].[RangoCriterio] ([CRTId], [Base], [MFechaHora], [MUsuarioId]) VALUES (1, N'OR', CAST(N'2020-04-01T00:00:00.000' AS DateTime), N'super');

-- INCESRION DE VALOR DE REFERENCIA
INSERT [dbo].[ValorReferencia] ([VARClave], [Descripcion], [Modificable], [MFechaHora], [MUsuarioId]) VALUES (N'TVARIACION', N'Tipos de cálculo para la tabla de Variación permitida.', 1, CAST(N'2020-04-01T00:00:00.000' AS DateTime), N'super');

-- INSECION DE VARVALORES
INSERT [dbo].[VARValor] ([VARClave], [VAVClave], [Descripcion], [Estatus], [MFechaHora], [MUsuarioId]) VALUES (N'TVARIACION', N'V', N'Valor', N'A', CAST(N'2020-04-22T18:57:34.000' AS DateTime), N'super');
INSERT [dbo].[VARValor] ([VARClave], [VAVClave], [Descripcion], [Estatus], [MFechaHora], [MUsuarioId]) VALUES (N'TVARIACION', N'P', N'Porcentaje', N'A', CAST(N'2020-04-22T18:57:34.000' AS DateTime), N'super');