BEGIN;

CREATE DATABASE texasholdem;
USE texasholdem;

-- Dumping structure for table texasholdem.friends
CREATE TABLE IF NOT EXISTS `friends` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `sender` varchar(20) DEFAULT NULL,
  `recipient` varchar(20) DEFAULT NULL,
  `status` enum('pending','accepted') DEFAULT NULL,
  `last_modified` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`id`),
  KEY `Sender` (`sender`) USING BTREE,
  KEY `Recipient` (`recipient`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table texasholdem.history
CREATE TABLE IF NOT EXISTS `history` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(20) DEFAULT NULL,
  `bets` int(11) NOT NULL DEFAULT 0,
  `table_name` varchar(20) DEFAULT NULL,
  `date` datetime DEFAULT current_timestamp(),
  `last_modified` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`id`),
  KEY `username` (`username`)
) ENGINE=InnoDB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table texasholdem.tables
CREATE TABLE IF NOT EXISTS `tables` (
  `tableNumber` int(3) NOT NULL AUTO_INCREMENT,
  `tableName` varchar(20) NOT NULL,
  `minPlayers` int(1) NOT NULL,
  `maxPlayers` int(1) NOT NULL,
  `turnLimit` int(3) NOT NULL,
  `private` tinyint(1) NOT NULL,
  `roomCode` varchar(20) NOT NULL,
  `last_modified` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`tableNumber`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

-- Dumping structure for table texasholdem.users
CREATE TABLE IF NOT EXISTS `users` (
  `username` varchar(20) NOT NULL,
  `token_count` int(10) DEFAULT NULL,
  `wins` int(4) DEFAULT NULL,
  `email` varchar(255) NOT NULL,
  `blockchain_ref` varchar(50) NOT NULL,
  `table_number` int(11) DEFAULT NULL,
  `last_modified` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`username`),
  KEY `table_number` (`table_number`),
  CONSTRAINT `FK_users_tables` FOREIGN KEY (`table_number`) REFERENCES `tables` (`tableNumber`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data exporting was unselected.

COMMIT;