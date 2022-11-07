USE [master]
GO
/****** Object:  Database [tvshowdb]    Script Date: 06/11/2022 23:13:59 ******/
CREATE DATABASE [tvshowdb]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'tvshowdb', FILENAME = N'C:\\DATA\tvshowdb.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'tvshowdb_log', FILENAME = N'C:\\DATA\tvshowdb_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [tvshowdb] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [tvshowdb].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [tvshowdb] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [tvshowdb] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [tvshowdb] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [tvshowdb] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [tvshowdb] SET ARITHABORT OFF 
GO
ALTER DATABASE [tvshowdb] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [tvshowdb] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [tvshowdb] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [tvshowdb] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [tvshowdb] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [tvshowdb] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [tvshowdb] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [tvshowdb] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [tvshowdb] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [tvshowdb] SET  ENABLE_BROKER 
GO
ALTER DATABASE [tvshowdb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [tvshowdb] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [tvshowdb] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [tvshowdb] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [tvshowdb] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [tvshowdb] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [tvshowdb] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [tvshowdb] SET RECOVERY FULL 
GO
ALTER DATABASE [tvshowdb] SET  MULTI_USER 
GO
ALTER DATABASE [tvshowdb] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [tvshowdb] SET DB_CHAINING OFF 
GO
ALTER DATABASE [tvshowdb] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [tvshowdb] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [tvshowdb] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [tvshowdb] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'tvshowdb', N'ON'
GO
ALTER DATABASE [tvshowdb] SET QUERY_STORE = OFF
GO
USE [tvshowdb]
GO
/****** Object:  Table [dbo].[castmember]    Script Date: 06/11/2022 23:13:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[castmember](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Bitrthday] [datetime2](7) NOT NULL,
	[TvShowId] [int] NULL,
 CONSTRAINT [PK_castmember] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[scrapes]    Script Date: 06/11/2022 23:13:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[scrapes](
	[Id] [int] NOT NULL,
	[TvShowId] [int] NOT NULL,
	[ScrapeDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_scrapes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tvshow]    Script Date: 06/11/2022 23:13:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tvshow](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[LastUpdated] [datetime2](7) NULL,
 CONSTRAINT [PK_tvshow] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Index [IX_castmember_TvShowId]    Script Date: 06/11/2022 23:13:59 ******/
CREATE NONCLUSTERED INDEX [IX_castmember_TvShowId] ON [dbo].[castmember]
(
	[TvShowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[castmember]  WITH CHECK ADD  CONSTRAINT [FK_castmember_tvshow_TvShowId] FOREIGN KEY([TvShowId])
REFERENCES [dbo].[tvshow] ([Id])
GO
ALTER TABLE [dbo].[castmember] CHECK CONSTRAINT [FK_castmember_tvshow_TvShowId]
GO
USE [master]
GO
ALTER DATABASE [tvshowdb] SET  READ_WRITE 
GO
