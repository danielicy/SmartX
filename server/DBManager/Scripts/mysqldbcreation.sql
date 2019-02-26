
--
-- Current Database: `mytweetdb`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `mytweetdb` /*!40100 DEFAULT CHARACTER SET utf8 */;

USE `mytweetdb`;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `roles` (
  `role_id` int(11) NOT NULL AUTO_INCREMENT,
  `role_name` longtext,
  PRIMARY KEY (`role_id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;


-- users Table
CREATE TABLE `users` (
  `userid` int(11) NOT NULL AUTO_INCREMENT,
  `user_name` longtext,
  `first_name` longtext,
  `last_name` longtext,
  `email` longtext,
  `hashed_password` longblob,
  `password_salt` longblob,
  `created_date` datetime NOT NULL,
  `modified_date` datetime DEFAULT NULL,
  `last_login` datetime DEFAULT NULL,
  `UserStatus` int(11) NOT NULL,
  `role_id` int(11) NOT NULL,
  `information` varchar(1000) DEFAULT NULL,
  `user_picture` blob,
  PRIMARY KEY (`userid`),
  KEY `IX_role_id` (`role_id`) USING HASH ,
  CONSTRAINT `FK_user_app_Roles_role_id` FOREIGN KEY (`role_id`) REFERENCES `roles` (`role_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;

CREATE TABLE `tweets` (
  `tweetid` INT NOT NULL AUTO_INCREMENT,
  `userid` INT NOT NULL,
  `created_date` DATETIME NOT NULL,
   `modified_date` DATETIME DEFAULT NULL,
  `tweet_content` longtext,
  PRIMARY KEY (`tweetid`),
  UNIQUE INDEX `idtweets_UNIQUE` (`tweetid` ASC),
  INDEX `FK_user_tweet_idx` (`userid` ASC),
  CONSTRAINT `FK_user_tweet`
    FOREIGN KEY (`userid`)
    REFERENCES `users` (`userid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);

		CREATE TABLE `contacts` (
  `userid` INT NOT NULL,
  `contactid` INT NOT NULL,
  PRIMARY KEY (`userid`, `contactid`),
    CONSTRAINT `FK_user`
    FOREIGN KEY (`userid`)
    REFERENCES `users` (`userid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `FK_contact`
    FOREIGN KEY (`contactid`)
    REFERENCES `users` (`userid`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION);


    
    -- fill roles table
    INSERT INTO `roles` (`role_id`, `role_name`) VALUES ('1', 'admin');
INSERT INTO `roles` (`role_id`, `role_name`) VALUES ('2', 'operator');
INSERT INTO `roles` (`role_id`, `role_name`) VALUES ('3', 'user');




