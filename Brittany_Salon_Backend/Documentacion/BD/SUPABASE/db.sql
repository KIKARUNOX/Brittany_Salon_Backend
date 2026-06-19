-- ============================================================
-- BRITTANY SALON - SCRIPT POSTGRESQL PARA SUPABASE
-- Ejecutar en SQL Editor de Supabase (la BD ya existe)
-- ============================================================

-- =========================
-- CATEGORY
-- =========================
CREATE TABLE IF NOT EXISTS Category (
    categoryId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    categoryName VARCHAR(50) NOT NULL UNIQUE,
    categoryDescription VARCHAR(255),
    isActive BOOLEAN DEFAULT TRUE
);

-- =========================
-- EMPLOYEE
-- =========================
CREATE TABLE IF NOT EXISTS Employee (
    employeeId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    phone VARCHAR(20),
    passwordHash VARCHAR(255) NOT NULL,
    specialty VARCHAR(100),
    imageUrl VARCHAR(255),
    isActive BOOLEAN DEFAULT TRUE,
    createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =========================
-- CLIENT
-- =========================
CREATE TABLE IF NOT EXISTS Client (
    clientId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    phone VARCHAR(20),
    passwordHash VARCHAR(255) NOT NULL,
    pendingBalance DECIMAL(10,2) DEFAULT 0,
    imageUrl VARCHAR(255),
    isActive BOOLEAN DEFAULT TRUE,
    createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =========================
-- SERVICE
-- =========================
CREATE TABLE IF NOT EXISTS Service (
    serviceId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    serviceName VARCHAR(100) NOT NULL,
    serviceDescription VARCHAR(255),
    price DECIMAL(10,2) NOT NULL,
    durationMinutes INTEGER NOT NULL,
    imageUrl VARCHAR(255),
    serviceType VARCHAR(50),
    isActive BOOLEAN DEFAULT TRUE
);

-- =========================
-- PRODUCT
-- =========================
CREATE TABLE IF NOT EXISTS Product (
    productId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    productName VARCHAR(100) NOT NULL,
    productDescription VARCHAR(255),
    price DECIMAL(10,2) NOT NULL,
    imageUrl VARCHAR(255),
    expirationDate DATE,
    isActive BOOLEAN DEFAULT TRUE,
    categoryId INTEGER NOT NULL,
    CONSTRAINT FK_Product_Category
        FOREIGN KEY (categoryId)
        REFERENCES Category(categoryId)
);

-- =========================
-- INVENTORY
-- =========================
CREATE TABLE IF NOT EXISTS Inventory (
    inventoryId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    productId INTEGER NOT NULL UNIQUE,
    quantity INTEGER NOT NULL,
    minimumStock INTEGER NOT NULL,
    maximumStock INTEGER NOT NULL,
    location VARCHAR(100),
    notes VARCHAR(255),
    lastUpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    isActive BOOLEAN DEFAULT TRUE,
    CONSTRAINT FK_Inventory_Product
        FOREIGN KEY (productId)
        REFERENCES Product(productId)
);

-- =========================
-- REVIEW
-- =========================
CREATE TABLE IF NOT EXISTS Review (
    reviewId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    comment VARCHAR(255),
    rating INTEGER CHECK (rating BETWEEN 1 AND 5),
    imageUrl VARCHAR(255),
    response VARCHAR(255),
    reviewDate DATE DEFAULT CURRENT_DATE,
    clientId INTEGER NOT NULL,
    employeeId INTEGER NULL,
    CONSTRAINT FK_Review_Client
        FOREIGN KEY (clientId)
        REFERENCES Client(clientId),
    CONSTRAINT FK_Review_Employee
        FOREIGN KEY (employeeId)
        REFERENCES Employee(employeeId)
);

-- =========================
-- EMPLOYEE SERVICE
-- =========================
CREATE TABLE IF NOT EXISTS EmployeeService (
    employeeServiceId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    employeeId INTEGER NOT NULL,
    serviceId INTEGER NOT NULL,
    CONSTRAINT FK_EmployeeService_Employee
        FOREIGN KEY (employeeId)
        REFERENCES Employee(employeeId),
    CONSTRAINT FK_EmployeeService_Service
        FOREIGN KEY (serviceId)
        REFERENCES Service(serviceId)
);

-- =========================
-- APPOINTMENT
-- =========================
CREATE TABLE IF NOT EXISTS Appointment (
    appointmentId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    appointmentDate DATE NOT NULL,
    startTime TIMESTAMP NOT NULL,
    endTime TIMESTAMP NOT NULL,
    appointmentStatus VARCHAR(50),
    totalCost DECIMAL(10,2),
    isActive BOOLEAN DEFAULT TRUE,
    clientId INTEGER NOT NULL,
    hairLengthOption INTEGER,
    CONSTRAINT FK_Appointment_Client
        FOREIGN KEY (clientId)
        REFERENCES Client(clientId)
);

-- =========================
-- APPOINTMENT SERVICE
-- =========================
CREATE TABLE IF NOT EXISTS AppointmentService (
    appointmentServiceId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    appointmentId INTEGER NOT NULL,
    serviceId INTEGER NOT NULL,
    servicePrice DECIMAL(10,2),
    CONSTRAINT FK_AppointmentService_Appointment
        FOREIGN KEY (appointmentId)
        REFERENCES Appointment(appointmentId),
    CONSTRAINT FK_AppointmentService_Service
        FOREIGN KEY (serviceId)
        REFERENCES Service(serviceId)
);

-- =========================
-- APPOINTMENT PRODUCT
-- =========================
CREATE TABLE IF NOT EXISTS AppointmentProduct (
    appointmentProductId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    appointmentId INTEGER NOT NULL,
    productId INTEGER NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT FK_AppointmentProduct_Appointment
        FOREIGN KEY (appointmentId)
        REFERENCES Appointment(appointmentId),
    CONSTRAINT FK_AppointmentProduct_Product
        FOREIGN KEY (productId)
        REFERENCES Product(productId),
    CONSTRAINT UQ_AppointmentProduct_AppointmentId_ProductId
        UNIQUE (appointmentId, productId)
);

-- =========================
-- PAYMENT
-- =========================
CREATE TABLE IF NOT EXISTS Payment (
    paymentId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    appointmentId INTEGER NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    paymentStatus VARCHAR(50),
    paymentDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    paymentMethod VARCHAR(50),
    isActive BOOLEAN DEFAULT TRUE,
    CONSTRAINT FK_Payment_Appointment
        FOREIGN KEY (appointmentId)
        REFERENCES Appointment(appointmentId)
);

-- =========================
-- REFRESH TOKEN
-- =========================
CREATE TABLE IF NOT EXISTS RefreshToken (
    refreshTokenId INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    token VARCHAR(500) NOT NULL,
    userId INTEGER NOT NULL,
    userType VARCHAR(50) NOT NULL,
    expirationDate TIMESTAMP NOT NULL,
    isRevoked BOOLEAN DEFAULT FALSE,
    createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    revokedAt TIMESTAMP NULL,
    revocationReason VARCHAR(255)
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_RefreshToken_Token ON RefreshToken (token);
CREATE INDEX IF NOT EXISTS IX_RefreshToken_UserId_UserType ON RefreshToken (userId, userType);

-- =========================
-- CATEGORIA POR DEFECTO
-- =========================
INSERT INTO Category (categoryName, categoryDescription)
VALUES ('General', 'Categoria por defecto')
ON CONFLICT (categoryName) DO NOTHING;
