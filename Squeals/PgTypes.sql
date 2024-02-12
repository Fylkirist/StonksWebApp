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
    CompanyId INTEGER,
    PDate DATETIME,
    Open REAL,
    Close REAL,
    High REAL,
    Low REAL
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
);

CREATE TABLE Portfolios(
    Id SERIAL PRIMARY KEY,
    UserId INTEGER,
    DateAdded DATE,
    PortfolioName VARCHAR(255)
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE TABLE PortfolioOrders(
    Id SERIAL PRIMARY KEY,
    PortfolioId INTEGER,
    CompanyId INTEGER,
    OrderType INTEGER,
    OrderDate DATE,
    OrderPrice REAL,
    OrderSize INTEGER
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
    FOREIGN KEY (PortfolioId) REFERENCES Portfolios(Id)
);
