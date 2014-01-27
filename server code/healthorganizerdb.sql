-- phpMyAdmin SQL Dump
-- version 4.0.4
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Jan 27, 2014 at 05:14 PM
-- Server version: 5.6.12-log
-- PHP Version: 5.4.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `healthorganizerdb`
--
CREATE DATABASE IF NOT EXISTS `healthorganizerdb` DEFAULT CHARACTER SET latin1 COLLATE latin1_swedish_ci;
USE `healthorganizerdb`;

-- --------------------------------------------------------

--
-- Table structure for table `address`
--

CREATE TABLE IF NOT EXISTS `address` (
  `PID` int(11) NOT NULL,
  `ZIP` int(11) NOT NULL,
  `Street` text NOT NULL,
  `Userid` int(11) NOT NULL,
  `TimeStamp` text NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `addresscity`
--

CREATE TABLE IF NOT EXISTS `addresscity` (
  `City` varchar(200) NOT NULL,
  `State` text NOT NULL,
  `Userid` int(11) NOT NULL,
  `TimeStamp` text NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`City`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `addressstate`
--

CREATE TABLE IF NOT EXISTS `addressstate` (
  `State` varchar(200) NOT NULL,
  `Country` text NOT NULL,
  `Userid` int(11) NOT NULL,
  `TimeStamp` text NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`State`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `addresszip`
--

CREATE TABLE IF NOT EXISTS `addresszip` (
  `ZIP` int(11) NOT NULL,
  `City` text NOT NULL,
  `Userid` int(11) NOT NULL,
  `TimeStamp` text NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`ZIP`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `medicaldetails`
--

CREATE TABLE IF NOT EXISTS `medicaldetails` (
  `PID` int(11) NOT NULL,
  `DateVisited` varchar(100) NOT NULL,
  `Age` int(11) NOT NULL,
  `BloodGlucose` int(11) DEFAULT NULL,
  `SystolicBP` int(11) DEFAULT NULL,
  `DiastolicBP` int(11) DEFAULT NULL,
  `DiseaseFound` text,
  `Height` double NOT NULL,
  `Weight` int(11) NOT NULL,
  `Symptoms` text NOT NULL,
  `BMI` double NOT NULL,
  `Userid` int(11) NOT NULL,
  `TimeStamp` text NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PID`,`DateVisited`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `medicaldetailsmedicine`
--

CREATE TABLE IF NOT EXISTS `medicaldetailsmedicine` (
  `TimeStamp` text NOT NULL,
  `PID` int(11) NOT NULL,
  `DateVisited` varchar(100) NOT NULL,
  `Medicine` varchar(500) NOT NULL,
  `Userid` int(11) NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PID`,`DateVisited`,`Medicine`),
  UNIQUE KEY `PID` (`PID`,`DateVisited`,`Medicine`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `medicaldetailsvaccine`
--

CREATE TABLE IF NOT EXISTS `medicaldetailsvaccine` (
  `TimeStamp` text NOT NULL,
  `PID` int(11) NOT NULL,
  `DateVisited` varchar(100) NOT NULL,
  `Vaccine` varchar(200) NOT NULL,
  `Userid` int(11) NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PID`,`DateVisited`,`Vaccine`),
  UNIQUE KEY `PID` (`PID`,`DateVisited`,`Vaccine`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `mutabledetails`
--

CREATE TABLE IF NOT EXISTS `mutabledetails` (
  `Married` varchar(2) NOT NULL,
  `Occupation` text NOT NULL,
  `FamilyBackground` text,
  `Email` text NOT NULL,
  `Mobile` bigint(20) unsigned NOT NULL,
  `EmMobile` bigint(20) unsigned NOT NULL,
  `PID` int(11) NOT NULL,
  `Userid` int(11) NOT NULL,
  `TimeStamp` text NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `mutabledetailsaddiction`
--

CREATE TABLE IF NOT EXISTS `mutabledetailsaddiction` (
  `PID` int(11) NOT NULL,
  `Addiction` varchar(500) NOT NULL,
  `Userid` int(11) NOT NULL,
  `TimeStamp` text NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PID`,`Addiction`),
  UNIQUE KEY `PID` (`PID`,`Addiction`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `mutabledetailsallergy`
--

CREATE TABLE IF NOT EXISTS `mutabledetailsallergy` (
  `PID` int(11) NOT NULL,
  `Allergy` varchar(500) NOT NULL,
  `Userid` int(11) NOT NULL,
  `TimeStamp` text NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PID`,`Allergy`),
  UNIQUE KEY `PID` (`PID`,`Allergy`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `mutabledetailsoperation`
--

CREATE TABLE IF NOT EXISTS `mutabledetailsoperation` (
  `PID` int(11) NOT NULL,
  `Operation` varchar(500) NOT NULL,
  `Userid` int(11) NOT NULL,
  `TimeStamp` text NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PID`,`Operation`),
  UNIQUE KEY `PID` (`PID`,`Operation`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `patient`
--

CREATE TABLE IF NOT EXISTS `patient` (
  `PID` int(11) NOT NULL AUTO_INCREMENT,
  `FirstName` text NOT NULL,
  `LastName` text NOT NULL,
  `BloodGroup` text NOT NULL,
  `Sex` text NOT NULL,
  `Birthday` text,
  `Image` text,
  `Userid` int(11) NOT NULL,
  `TimeStamp` text NOT NULL,
  `ServerTimestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`PID`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=3 ;

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE IF NOT EXISTS `users` (
  `Userid` int(11) NOT NULL AUTO_INCREMENT,
  `Username` varchar(100) NOT NULL,
  `Password` varchar(40) NOT NULL,
  `Email` varchar(50) NOT NULL,
  `Organisation` varchar(100) NOT NULL,
  `Auth_Token` varchar(100) NOT NULL,
  `Confirmation` varchar(10) NOT NULL DEFAULT 'false',
  PRIMARY KEY (`Userid`),
  UNIQUE KEY `Username_2` (`Username`),
  UNIQUE KEY `Email_2` (`Email`),
  UNIQUE KEY `Organisation` (`Organisation`),
  FULLTEXT KEY `Password` (`Password`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 AUTO_INCREMENT=1 ;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `address`
--
ALTER TABLE `address`
  ADD CONSTRAINT `address_ibfk_1` FOREIGN KEY (`PID`) REFERENCES `patient` (`PID`) ON DELETE CASCADE;

--
-- Constraints for table `medicaldetails`
--
ALTER TABLE `medicaldetails`
  ADD CONSTRAINT `medicaldetails_ibfk_1` FOREIGN KEY (`PID`) REFERENCES `patient` (`PID`) ON DELETE CASCADE;

--
-- Constraints for table `medicaldetailsmedicine`
--
ALTER TABLE `medicaldetailsmedicine`
  ADD CONSTRAINT `medicaldetailsmedicine_ibfk_1` FOREIGN KEY (`PID`, `DateVisited`) REFERENCES `medicaldetails` (`PID`, `DateVisited`) ON DELETE CASCADE;

--
-- Constraints for table `medicaldetailsvaccine`
--
ALTER TABLE `medicaldetailsvaccine`
  ADD CONSTRAINT `medicaldetailsvaccine_ibfk_1` FOREIGN KEY (`PID`, `DateVisited`) REFERENCES `medicaldetails` (`PID`, `DateVisited`) ON DELETE CASCADE;

--
-- Constraints for table `mutabledetails`
--
ALTER TABLE `mutabledetails`
  ADD CONSTRAINT `mutabledetails_ibfk_1` FOREIGN KEY (`PID`) REFERENCES `patient` (`PID`) ON DELETE CASCADE;

--
-- Constraints for table `mutabledetailsaddiction`
--
ALTER TABLE `mutabledetailsaddiction`
  ADD CONSTRAINT `mutabledetailsaddiction_ibfk_1` FOREIGN KEY (`PID`) REFERENCES `mutabledetails` (`PID`) ON DELETE CASCADE;

--
-- Constraints for table `mutabledetailsallergy`
--
ALTER TABLE `mutabledetailsallergy`
  ADD CONSTRAINT `mutabledetailsallergy_ibfk_1` FOREIGN KEY (`PID`) REFERENCES `mutabledetails` (`PID`) ON DELETE CASCADE;

--
-- Constraints for table `mutabledetailsoperation`
--
ALTER TABLE `mutabledetailsoperation`
  ADD CONSTRAINT `mutabledetailsoperation_ibfk_1` FOREIGN KEY (`PID`) REFERENCES `mutabledetails` (`PID`) ON DELETE CASCADE;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
