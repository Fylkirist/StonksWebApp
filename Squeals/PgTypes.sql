CREATE TABLE Companies (
    Id SERIAL PRIMARY KEY,
    CName VARCHAR(255),
    CIK INT,
    Ticker VARCHAR(255),
    FiscalYearEnd DATE,
    Shares INT,
    Sector VARCHAR(50)
);

ALTER TABLE Companies ADD CONSTRAINT unique_cik_constraint UNIQUE (CIK);


CREATE TABLE Users(
    Id SERIAL PRIMARY KEY,
    Email VARCHAR(255),
    Pass VARCHAR(255),
    DateCreated DATETIME,
    Watchlist TEXT,
    UserRole VARCHAR(10),
    Validated INTEGER
);

CREATE TABLE Filings(
    Id SERIAL PRIMARY KEY,
    CompanyId INTEGER NOT NULL,
    Link VARCHAR(2048) NOT NULL,
    FilingDate DATE NOT NULL,
    FilingType VARCHAR(30) NOT NULL,
    Org VARCHAR(10) NOT NULL
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)  
);

CREATE TABLE Prices(
    Id SERIAL PRIMARY KEY,
    CompanyId INTEGER NOT NULL,
    PDate DATETIME NOT NULL,
    Open REAL NOT NULL,
    Close REAL NOT NULL,
    High REAL NOT NULL,
    Low REAL NOT NULL
    FOREIGN KEY(CompanyId) REFERENCES Companies(Id)
);
