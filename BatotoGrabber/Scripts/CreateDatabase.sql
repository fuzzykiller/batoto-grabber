BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS `Series` (
	`PrimaryName`	TEXT NOT NULL,
	`Type`	TEXT,
	`Status`	TEXT,
	`Description`	TEXT,
	`Image`	BLOB,
	PRIMARY KEY(`PrimaryName`)
);
CREATE TABLE IF NOT EXISTS `Groups` (
	`Id`	INTEGER,
	`Name`	TEXT NOT NULL,
	`Website`	TEXT,
	`Description`	TEXT,
	`Delay`	TEXT,
	PRIMARY KEY(`Id`)
);
CREATE TABLE IF NOT EXISTS `Genres` (
	`Name`	TEXT NOT NULL,
	PRIMARY KEY(`Name`)
);
CREATE TABLE IF NOT EXISTS `Creators` (
	`Name`	TEXT NOT NULL,
	PRIMARY KEY(`Name`)
);
CREATE TABLE IF NOT EXISTS `Chapters` (
	`Id`	INTEGER,
	`Title`	TEXT NOT NULL,
	`Language`	TEXT,
	`Contributor`	TEXT,
	`Date`	TEXT,
	`Series`	TEXT NOT NULL,
	`LastRead`	TEXT,
	PRIMARY KEY(`Id`),
	FOREIGN KEY(`Series`) REFERENCES `Series`(`PrimaryName`)
);
CREATE TABLE IF NOT EXISTS `ChapterGroup` (
	`ChapterId`	INTEGER,
	`GroupId`	INTEGER,
	FOREIGN KEY(`ChapterId`) REFERENCES `Chapters`(`Id`),
	FOREIGN KEY(`GroupId`) REFERENCES `Groups`(`Id`)
);
CREATE TABLE IF NOT EXISTS `SeriesAuthor` (
	`SeriesName`	TEXT,
	`CreatorName`	TEXT,
	FOREIGN KEY(`SeriesName`) REFERENCES `Series`(`PrimaryName`),
	FOREIGN KEY(`CreatorName`) REFERENCES `Creators`(`Name`)
);
CREATE TABLE IF NOT EXISTS `SeriesArtist` (
	`SeriesName`	TEXT,
	`CreatorName`	TEXT,
	FOREIGN KEY(`SeriesName`) REFERENCES `Series`(`PrimaryName`),
	FOREIGN KEY(`CreatorName`) REFERENCES `Creators`(`Name`)
);
CREATE TABLE IF NOT EXISTS `SeriesGenre` (
	`SeriesName`	TEXT,
	`GenreName`	TEXT,
	FOREIGN KEY(`SeriesName`) REFERENCES `Series`(`PrimaryName`),
	FOREIGN KEY(`GenreName`) REFERENCES `Genres`(`Name`)
);
COMMIT;
