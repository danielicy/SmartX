
--
-- Current Database: `mytweetdb`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ mytweetdb /*!40100 DEFAULT CHARACTER SET utf8 */;

USE [mytweetdb];

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS [roles];
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE roles (
  [role_id] int NOT NULL IDENTITY,
  [role_name] varchar(max),
  PRIMARY KEY ([role_id])
)  ;
/*!40101 SET character_set_client = @saved_cs_client */;


-- users Table
CREATE TABLE users (
  [userid] int NOT NULL IDENTITY,
  [user_name] varchar(max),
  [first_name] varchar(max),
  [last_name] varchar(max),
  [email] varchar(max),
  [hashed_password] varbinary(max),
  [password_salt] varbinary(max),
  [created_date] datetime2(0) NOT NULL,
  [modified_date] datetime2(0) DEFAULT NULL,
  [last_login] datetime2(0) DEFAULT NULL,
  [UserStatus] int NOT NULL,
  [role_id] int NOT NULL,
  [information] varchar(1000) DEFAULT NULL,
  [user_picture] varbinary(max),
  PRIMARY KEY ([userid])
  ,
  CONSTRAINT [FK_user_app_Roles_role_id] FOREIGN KEY ([role_id]) REFERENCES roles ([role_id]) ON DELETE CASCADE ON UPDATE CASCADE
)  ;

CREATE INDEX [IX_role_id] ON users ([role_id]);

CREATE TABLE tweets (
  [tweetid] INT NOT NULL IDENTITY,
  [userid] INT NOT NULL,
  [created_date] DATETIME2(0) NOT NULL,
   [modified_date] DATETIME2(0) DEFAULT NULL,
  [tweet_content] varchar(max),
  PRIMARY KEY ([tweetid]),
  CONSTRAINT [idtweets_UNIQUE] UNIQUE  ([tweetid] ASC)
 ,
  CONSTRAINT [FK_user_tweet]
    FOREIGN KEY ([userid])
    REFERENCES users ([userid])
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);

CREATE INDEX [FK_user_tweet_idx] ON tweets ([userid] ASC);

		CREATE TABLE contacts (
  [userid] INT NOT NULL,
  [contactid] INT NOT NULL,
  PRIMARY KEY ([userid], [contactid]),
    CONSTRAINT [FK_user]
    FOREIGN KEY ([userid])
    REFERENCES users ([userid])
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT [FK_contact]
    FOREIGN KEY ([contactid])
    REFERENCES users ([userid])
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);


   
    -- fill roles table
 SET IDENTITY_INSERT roles ON
 INSERT INTO roles ([role_id], [role_name]) VALUES ('1', 'admin');
INSERT INTO roles ([role_id], [role_name]) VALUES ('2', 'operator');
INSERT INTO roles ([role_id], [role_name]) VALUES ('3', 'user');
 SET IDENTITY_INSERT roles OFF


