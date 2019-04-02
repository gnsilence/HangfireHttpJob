-- ----------------------------

-- Table structure for `Job`

-- ----------------------------

CREATE TABLE `<tableprefix>_Job` (

  `Id` int(11) NOT NULL AUTO_INCREMENT,

  `StateId` int(11) DEFAULT NULL,

  `StateName` varchar(20) DEFAULT NULL,

  `InvocationData` longtext NOT NULL,

  `Arguments` longtext NOT NULL,

  `CreatedAt` datetime NOT NULL,

  `ExpireAt` datetime DEFAULT NULL,

  PRIMARY KEY (`Id`),

  KEY `IX_Job_StateName` (`StateName`)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;





-- ----------------------------

-- Table structure for `Counter`

-- ----------------------------

CREATE TABLE `<tableprefix>_Counter` (

  `Id` int(11) NOT NULL AUTO_INCREMENT,

  `Key` varchar(100) NOT NULL,

  `Value` int(11) NOT NULL,

  `ExpireAt` datetime DEFAULT NULL,

  PRIMARY KEY (`Id`),

  KEY `IX_Counter_Key` (`Key`)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;





CREATE TABLE `<tableprefix>_AggregatedCounter` (

	Id int(11) NOT NULL AUTO_INCREMENT,

	`Key` varchar(100) NOT NULL,

	`Value` int(11) NOT NULL,

	ExpireAt datetime DEFAULT NULL,

	PRIMARY KEY (`Id`),

	UNIQUE KEY `IX_CounterAggregated_Key` (`Key`)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;





-- ----------------------------

-- Table structure for `DistributedLock`

-- ----------------------------

CREATE TABLE `<tableprefix>_DistributedLock` (

  `Resource` varchar(100) NOT NULL,

  `CreatedAt` datetime NOT NULL

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;





-- ----------------------------

-- Table structure for `Hash`

-- ----------------------------

CREATE TABLE `<tableprefix>_Hash` (

  `Id` int(11) NOT NULL AUTO_INCREMENT,

  `Key` varchar(100) NOT NULL,

  `Field` varchar(40) NOT NULL,

  `Value` longtext,

  `ExpireAt` datetime DEFAULT NULL,

  PRIMARY KEY (`Id`),

  UNIQUE KEY `IX_Hash_Key_Field` (`Key`,`Field`)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;





-- ----------------------------

-- Table structure for `JobParameter`

-- ----------------------------

CREATE TABLE `<tableprefix>_JobParameter` (

  `Id` int(11) NOT NULL AUTO_INCREMENT,

  `JobId` int(11) NOT NULL,

  `Name` varchar(40) NOT NULL,

  `Value` longtext,



  PRIMARY KEY (`Id`),

  CONSTRAINT `IX_JobParameter_JobId_Name` UNIQUE (`JobId`,`Name`),

  KEY `FK_JobParameter_Job` (`JobId`)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;



-- ----------------------------

-- Table structure for `JobQueue`

-- ----------------------------

CREATE TABLE `<tableprefix>_JobQueue` (

  `Id` int(11) NOT NULL AUTO_INCREMENT,

  `JobId` int(11) NOT NULL,

  `Queue` varchar(50) NOT NULL,

  `FetchedAt` datetime DEFAULT NULL,

  `FetchToken` varchar(36) DEFAULT NULL,

  

  PRIMARY KEY (`Id`),

  INDEX `IX_JobQueue_QueueAndFetchedAt` (`Queue`,`FetchedAt`)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;



-- ----------------------------

-- Table structure for `JobState`

-- ----------------------------

CREATE TABLE `<tableprefix>_JobState` (

  `Id` int(11) NOT NULL AUTO_INCREMENT,

  `JobId` int(11) NOT NULL,

  `Name` varchar(20) NOT NULL,

  `Reason` varchar(100) DEFAULT NULL,

  `CreatedAt` datetime NOT NULL,

  `Data` longtext,

  PRIMARY KEY (`Id`),

  KEY `FK_JobState_Job` (`JobId`)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;



-- ----------------------------

-- Table structure for `Server`

-- ----------------------------

CREATE TABLE `<tableprefix>_Server` (

  `Id` varchar(100) NOT NULL,

  `Data` longtext NOT NULL,

  `LastHeartbeat` datetime DEFAULT NULL,

  PRIMARY KEY (`Id`)

) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;





-- ----------------------------

-- Table structure for `Set`

-- ----------------------------

CREATE TABLE `<tableprefix>_Set` (

  `Id` int(11) NOT NULL AUTO_INCREMENT,

  `Key` varchar(100) NOT NULL,

  `Value` varchar(100) NOT NULL,

  `Score` float NOT NULL,

  `ExpireAt` datetime DEFAULT NULL,

  PRIMARY KEY (`Id`),

  UNIQUE KEY `IX_Set_Key_Value` (`Key`,`Value`)

) ENGINE=InnoDB  CHARSET=utf8mb4;







CREATE TABLE `<tableprefix>_State`

(

	Id int(11) NOT NULL AUTO_INCREMENT,

	JobId int(11) NOT NULL,

	Name varchar(20) NOT NULL,

	Reason varchar(100) NULL,

	CreatedAt datetime NOT NULL,

	Data longtext NULL,

	PRIMARY KEY (`Id`),

	KEY `FK_HangFire_State_Job` (`JobId`)

) ENGINE=InnoDB  CHARSET=utf8mb4;



CREATE TABLE `<tableprefix>_List`

(

	`Id` int(11) NOT NULL AUTO_INCREMENT,

	`Key` varchar(100) NOT NULL,

	`Value` longtext NULL,

	`ExpireAt` datetime NULL,

	PRIMARY KEY (`Id`)

) ENGINE=InnoDB  CHARSET=utf8mb4;