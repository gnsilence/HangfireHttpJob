
/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`hangfire_fat` /*!40100 DEFAULT CHARACTER SET utf8mb4 */;

USE `hangfire_fat`;

/*Table structure for table `aggregatedcounter` */

DROP TABLE IF EXISTS `aggregatedcounter`;

CREATE TABLE `aggregatedcounter` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `Key` VARCHAR(100) NOT NULL,
  `Value` INT(11) NOT NULL,
  `ExpireAt` DATETIME DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_CounterAggregated_Key` (`Key`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `aggregatedcounter` */

/*Table structure for table `counter` */

DROP TABLE IF EXISTS `counter`;

CREATE TABLE `counter` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `Key` VARCHAR(100) NOT NULL,
  `Value` INT(11) NOT NULL,
  `ExpireAt` DATETIME DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Counter_Key` (`Key`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `counter` */

/*Table structure for table `distributedlock` */

DROP TABLE IF EXISTS `distributedlock`;

CREATE TABLE `distributedlock` (
  `Resource` VARCHAR(100) NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `distributedlock` */

/*Table structure for table `hash` */

DROP TABLE IF EXISTS `hash`;

CREATE TABLE `hash` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `Key` VARCHAR(100) NOT NULL,
  `Field` VARCHAR(40) NOT NULL,
  `Value` LONGTEXT,
  `ExpireAt` DATETIME(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Hash_Key_Field` (`Key`,`Field`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `hash` */

/*Table structure for table `job` */

DROP TABLE IF EXISTS `job`;

CREATE TABLE `job` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `StateId` INT(11) DEFAULT NULL,
  `StateName` VARCHAR(20) DEFAULT NULL,
  `InvocationData` LONGTEXT NOT NULL,
  `Arguments` LONGTEXT NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  `ExpireAt` DATETIME(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Job_StateName` (`StateName`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `job` */

/*Table structure for table `jobparameter` */

DROP TABLE IF EXISTS `jobparameter`;

CREATE TABLE `jobparameter` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `JobId` INT(11) NOT NULL,
  `Name` VARCHAR(40) NOT NULL,
  `Value` LONGTEXT,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_JobParameter_JobId_Name` (`JobId`,`Name`),
  KEY `FK_JobParameter_Job` (`JobId`),
  CONSTRAINT `FK_JobParameter_Job` FOREIGN KEY (`JobId`) REFERENCES `job` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `jobparameter` */

/*Table structure for table `jobqueue` */

DROP TABLE IF EXISTS `jobqueue`;

CREATE TABLE `jobqueue` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `JobId` INT(11) NOT NULL,
  `Queue` VARCHAR(50) NOT NULL,
  `FetchedAt` DATETIME(6) DEFAULT NULL,
  `FetchToken` VARCHAR(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_JobQueue_QueueAndFetchedAt` (`Queue`,`FetchedAt`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `jobqueue` */

/*Table structure for table `jobstate` */

DROP TABLE IF EXISTS `jobstate`;

CREATE TABLE `jobstate` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `JobId` INT(11) NOT NULL,
  `Name` VARCHAR(20) NOT NULL,
  `Reason` VARCHAR(100) DEFAULT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  `Data` LONGTEXT,
  PRIMARY KEY (`Id`),
  KEY `FK_JobState_Job` (`JobId`),
  CONSTRAINT `FK_JobState_Job` FOREIGN KEY (`JobId`) REFERENCES `job` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `jobstate` */

/*Table structure for table `list` */

DROP TABLE IF EXISTS `list`;

CREATE TABLE `list` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `Key` VARCHAR(100) NOT NULL,
  `Value` LONGTEXT,
  `ExpireAt` DATETIME(6) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `list` */

/*Table structure for table `server` */

DROP TABLE IF EXISTS `server`;

CREATE TABLE `server` (
  `Id` VARCHAR(100) NOT NULL,
  `Data` LONGTEXT NOT NULL,
  `LastHeartbeat` DATETIME(6) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `server` */

/*Table structure for table `set` */

DROP TABLE IF EXISTS `set`;

CREATE TABLE `set` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `Key` VARCHAR(100) NOT NULL,
  `Value` VARCHAR(255) NOT NULL,
  `Score` FLOAT NOT NULL,
  `ExpireAt` DATETIME DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Set_Key_Value` (`Key`,`Value`)
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `set` */

/*Table structure for table `state` */

DROP TABLE IF EXISTS `state`;

CREATE TABLE `state` (
  `Id` INT(11) NOT NULL AUTO_INCREMENT,
  `JobId` INT(11) NOT NULL,
  `Name` VARCHAR(20) NOT NULL,
  `Reason` VARCHAR(100) DEFAULT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  `Data` LONGTEXT,
  PRIMARY KEY (`Id`),
  KEY `FK_HangFire_State_Job` (`JobId`),
  CONSTRAINT `FK_HangFire_State_Job` FOREIGN KEY (`JobId`) REFERENCES `job` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=INNODB DEFAULT CHARSET=utf8mb4;

/*Data for the table `state` */

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
