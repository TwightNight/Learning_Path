# INTRODUCTION TO SQL

## What is SQL

* A **Database** is a structured collection of data that is stored and organized in a way that makes it easy to **access, manage, and retrieve**.
  In relational databases, data is typically stored in **tables**, which consist of **rows (records)** and **columns (fields)**.

* **SQL (Structured Query Language)** is a **standard programming language** used to interact with relational databases.
  It allows users to **communicate with a Database Management System (DBMS)** to perform various operations on data.

* With SQL, you can:

  * **Query data** (retrieve specific information)
  * **Insert data** (add new records)
  * **Update data** (modify existing records)
  * **Delete data** (remove records)
  * **Manage database structure** (create, modify, or delete tables and schemas)
---

## Key Concept

> SQL is **not a database** — it is a **language used to interact with databases**.

* Instead of accessing raw data directly:

  * You write SQL queries
  * The DBMS processes them
  * The result is returned to you
---

## What is a DBMS

* A **DBMS (Database Management System)** is software that acts as an **intermediary between users/applications and the database**.

* It is responsible for:

  * Storing and organizing data
  * Processing SQL queries
  * Ensuring **data integrity, consistency, and security**
  * Managing **concurrent access** from multiple users

* Examples of DBMS include:

  * MySQL
  * Microsoft SQL Server
  * PostgreSQL
  * Oracle Database

---

## What is a Database Server

* A **database server** is a system (hardware and software) that runs a DBMS and provides database services to clients over a network.

* It receives SQL requests from client applications, processes them using the DBMS, and returns the results.

* In a typical architecture:

  * **Client** → sends SQL query
  * **Server (DBMS)** → processes query
  * **Database** → stores the actual data

---

## How They Work Together

> SQL is the language used to communicate with a database through a DBMS running on a server.

1. The client sends an SQL query
2. The database server receives the request
3. The DBMS processes the query
4. The result is returned to the client

---
## Database Types

### 1. Relational Database (SQL)

*   **Term Definition:** A Relational Database Management System (RDBMS) is a data storage and retrieval system based on the relational model proposed by Edgar F. Codd in 1970. This model organizes data into highly structured sets of entities, strictly adhering to normalization principles.
*   **How it works:** Data is mapped into a two-dimensional space consisting of tables (relations). Each table comprises rows (tuples/records—representing a unique entity) and columns (attributes/fields—representing the entity's properties). Tables are interconnected via Primary Keys and Foreign Keys. Query operations are executed through Structured Query Language (SQL). The system strictly enforces ACID (Atomicity, Consistency, Isolation, Durability) properties to guarantee data integrity during transactions.
*   **Why use it:** It provides absolute data consistency and accuracy. The normalized structure minimizes data redundancy and prevents anomalies during data insertion, updating, or deletion.
*   **Where to use:** Ideally suited for systems requiring complex transactional integrity, such as: Financial and banking systems, Enterprise Resource Planning (ERP), Customer Relationship Management (CRM), and accounting software.
*   **Practical Examples:** PostgreSQL, MySQL, Oracle Database, Microsoft SQL Server.

### 2. Key-Value Database (NoSQL)

*   **Term Definition:** This is the most minimalist non-relational database model, where each record is represented as a collection of key-value pairs without a fixed schema structure (schema-less).
*   **How it works:** It operates on the concept of a large-scale distributed hash table. The "Key" serves as a unique identifier (typically a string) for direct retrieval of the "Value." The "Value" acts as an opaque data block to the system, which can take any format (number, string, JSON, BLOB) without the database needing to parse or understand its internal structure.
*   **Why use it:** The O(1) time complexity of the hash table architecture delivers exceptionally low retrieval latency (measured in milliseconds). Due to its loose structure, it possesses near-linear horizontal scalability and the ability to process extremely high-throughput workloads.
*   **Where to use:** Caching, User session management, Shopping cart storage in e-commerce, Real-time leaderboards.
*   **Practical Examples:** Redis, Amazon DynamoDB, Memcached, Riak.

### 3. Column-Based / Wide-Column Database (NoSQL)

*   **Term Definition:** Also known as a Wide-Column Store, this is a database management system that stores data by columns rather than by rows (row-oriented), specifically designed for managing and analyzing large-scale multidimensional datasets (Big Data).
*   **How it works:** Instead of storing an entire row of data in contiguous memory blocks, a column-oriented database groups values belonging to the same column together. Its structure is typically based on the concept of "Column Families," where a row is not required to have the exact same column structure as other rows. This allows the storage of millions of sparse data columns within a single table without wasting space on null values.
*   **Why use it:** Exceptionally optimized for analytical aggregation queries, as the system only needs to read data from specified columns rather than loading entire rows. Grouping data of the same type also creates ideal conditions for data compression algorithms to operate at maximum efficiency.
*   **Where to use:** Time-series data analysis, Data Warehousing/OLAP, Log management, Telecommunications systems, and the Internet of Things (IoT).
*   **Practical Examples:** Apache Cassandra, Apache HBase, Google Bigtable.

### 4. Graph Database (NoSQL)

*   **Term Definition:** A non-relational database model that applies Graph Theory from mathematics to represent, store, and navigate complex interconnections between data points.
*   **How it works:** The data structure is built upon three ontological components: Nodes (representing entities like people, objects, locations), Edges (representing directed or undirected relationships between nodes), and Properties (metadata attached to both nodes and edges). Instead of executing resource-intensive JOIN operations like an RDBMS, a graph database performs "graph traversal" along the edges, keeping computational costs stable regardless of the graph's overall scale.
*   **Why use it:** It provides the capability to map and query intricate, multi-level relationship networks rapidly and intuitively. It essentially treats the "relationship" as a first-class citizen of the data system.
*   **Where to use:** Recommendation engines, Social networks, Fraud detection systems, Knowledge graphs, Supply chain optimization.
*   **Practical Examples:** Neo4j, Amazon Neptune, ArangoDB, TigerGraph.

### 5. Document Database (NoSQL)

*   **Term Definition:** A database system designed to manage semi-structured or loosely structured data repositories, utilizing the concept of a "Document" as the fundamental storage unit.
*   **How it works:** Data is encoded and stored in standard document formats, typically JSON, BSON (Binary JSON), or XML. Each document is an independent entity containing key-value pairs, arrays, or nested documents. The system is polymorphic, meaning documents within the same collection can have entirely different structures.
*   **Why use it:** It offers absolute schema flexibility, allowing developers to adapt data modeling seamlessly alongside application progression without the need for rigid structural migrations. Data is stored intuitively, closely mirroring the objects used in modern application programming languages.
*   **Where to use:** Content Management Systems (CMS), Multi-attribute user profiles, Product catalogs with dynamic attributes, Web and mobile applications requiring Agile development cycles.
*   **Practical Examples:** MongoDB, Couchbase, Amazon DocumentDB.

---
## Relational Database Structure 



The structure of an SQL (Relational) database is fundamentally rooted in the **relational data model**. It is characterized by a highly organized, hierarchical, and schema-bound architecture designed to guarantee data integrity, eliminate redundancy, and facilitate complex querying.

To understand the structure of an SQL database academically, it must be analyzed through its **Logical Hierarchy**, **Relational Components**, and **Physical Architecture**.

---

### 1. Logical Hierarchical Structure (Top-Down)
This is how data is logically organized and abstracted for the user and database administrator.

*   **Database Instance / Server:** The top-level operational environment (the RDBMS software itself, e.g., an active SQL Server or PostgreSQL process) that manages multiple databases, memory allocation, and background processes.
*   **Database (Catalog):** An independent, logical container that holds a complete set of schemas, tables, users, and relational objects.
*   **Schema:** A namespace or logical grouping within a database. It acts as a structural boundary to organize database objects (tables, views) into manageable modules and control user access (e.g., `sales.customers` vs. `hr.employees`).
*   **Table (Relation):** The primary unit of data storage. It is a two-dimensional matrix representing a specific entity (e.g., "Employees").
    *   **Column (Attribute / Field):** The vertical dimension of a table. It defines a specific property of the entity (e.g., "DateOfBirth") and strictly enforces a **Data Type** (e.g., INT, VARCHAR, DATE) and constraints.
    *   **Row (Tuple / Record):** The horizontal dimension. It represents a single, unique, physical instance of the entity (e.g., one specific employee).

### 2. Core Relational Components (The "Connective Tissue")
These structural elements enforce the mathematical rules of the relational model, ensuring accuracy and consistency.

*   **Primary Key (PK):** A designated column (or combination of columns) that uniquely identifies every row within a table. It mathematically enforces **Entity Integrity** (ensuring no duplicate records exist and the key is never NULL).
*   **Foreign Key (FK):** A column in one table that references the Primary Key of another table. This establishes the "relationship" in a relational database and enforces **Referential Integrity** (ensuring that relationships between tables remain consistent and valid; e.g., an order cannot be placed by a non-existent customer).
*   **Constraints:** Strict rules applied at the column level to restrict the types of data that can go into a table. Common constraints include `NOT NULL` (requires a value), `UNIQUE` (prevents duplicate values in non-primary key columns), and `CHECK` (validates data against a specific condition).
*   **Indexes:** Auxiliary data structures (most commonly **B-Trees** or Hash tables) built on top of columns. They do not store the core data but act like an index in a book, significantly accelerating data retrieval times (O(log n) complexity) at the expense of slower write operations.

### 3. Abstract and Programmable Objects
SQL databases go beyond simple storage; they encapsulate logic and abstraction within their structure.

*   **Views:** Virtual tables derived from the result set of a pre-defined SQL query. They do not store data themselves but provide an abstracted layer to simplify complex joins, aggregate data, or restrict user access to specific columns (security).
*   **Stored Procedures:** Pre-compiled batches of SQL statements and procedural logic (using control-of-flow languages like PL/pgSQL or T-SQL) stored directly within the database architecture. They promote code reusability and optimize network traffic.
*   **Triggers:** Event-driven, automated SQL scripts that execute implicitly when specific Data Manipulation Language (DML) actions occur (e.g., automatically updating a "LastModified" timestamp whenever a row is `UPDATED`).

### 4. Physical Storage Structure (Bottom-Up)
Beneath the logical abstraction, the RDBMS structures data physically on the storage disk.

*   **Pages / Blocks:** The most granular unit of physical data storage (typically 4KB or 8KB in size). Every row of a table is physically written into these pages.
*   **Extents:** A logical collection of contiguous pages (e.g., 8 pages make up one extent) used by the database engine to manage disk space allocation efficiently.
*   **Data Files:** The physical operating system files (e.g., `.mdf` or `.ibd`) where the pages, extents, and actual table data reside.
*   **Transaction Log Files (WAL - Write-Ahead Logging):** Crucial structural files (e.g., `.ldf`) that record all transaction modifications *before* they are written to the main data files. This structure is what guarantees the **Durability (D)** in ACID properties, allowing the database to recover from unexpected system failures.

---
## Types of SQL Commands

From an academic and system architecture perspective, Structured Query Language (SQL) is not a monolithic block of commands but is categorized into specific sub-languages. This division is based on semantics and the command's operational impact on the database: whether it alters the structural framework (metadata) or manipulates the actual information (data instances).

Below is a detailed academic analysis of the three fundamental SQL command groups:

---

### 1. DDL (Data Definition Language)

*   **Term Definition:** DDL is a subset of SQL used to define, modify, and manage the schema structure and metadata of database objects (such as tables, views, indexes, and stored procedures).
*   **How it works:** DDL commands do not interact directly with data rows (records) but rather interface with the Data Dictionary or System Catalog. When a DDL command is executed, it fundamentally alters the physical or logical architecture of the data container. In many Database Management Systems (like Oracle), DDL operations are auto-committed, meaning their structural changes are immediate, permanent, and cannot be reversed (rolled back) through standard transaction management.
*   **Why use it:** To establish the architectural blueprint of the database before any data can be ingested, and to facilitate schema evolution as business requirements change over time.
*   **Where to use:** Initializing a new database for a project, adding a new column to an existing table to accommodate new attributes, or completely dropping obsolete tables to reclaim storage space.
*   **Representative Commands & Examples:**
    *   **`CREATE`**: Initializes a brand-new object. *(Ex: `CREATE TABLE Customers (ID INT, Name VARCHAR(50));`)*
    *   **`ALTER`**: Modifies the structure of an existing object. *(Ex: `ALTER TABLE Customers ADD Email VARCHAR(100);`)*
    *   **`DROP`**: Completely eradicates an object and its associated metadata from the system. *(Ex: `DROP TABLE Customers;`)*

### 2. DML (Data Manipulation Language)

*   **Term Definition:** DML is a specialized command group designed exclusively for inserting, updating, and managing physical data instances (i.e., the rows/records) stored within the schema structures previously defined by DDL.
*   **How it works:** DML commands directly intervene with the data residing in memory pages. The most critical characteristic of DML is that it operates within the boundaries of a **Transaction**. Modifications are initially written to the Buffer Cache and the Write-Ahead Log (WAL), meaning they can be strictly controlled: either committed permanently (via `COMMIT`) or reversed if an anomaly occurs (via `ROLLBACK`), thereby guaranteeing ACID compliance.
*   **Why use it:** To maintain, mutate, and manage the lifecycle of the actual data generated during daily application operations (fulfilling the Create, Update, and Delete aspects of the CRUD paradigm).
*   **Where to use:** Processing routine operational tasks: Recording a newly placed order from an e-commerce website (INSERT), updating an order's status to "Shipped" (UPDATE), or removing a deactivated user account (DELETE).
*   **Representative Commands & Examples:**
    *   **`INSERT`**: Injects a new record into a table. *(Ex: `INSERT INTO Customers (ID, Name) VALUES (1, 'John Doe');`)*
    *   **`UPDATE`**: Modifies the values of one or multiple existing records. *(Ex: `UPDATE Customers SET Name = 'Jane Doe' WHERE ID = 1;`)*
    *   **`DELETE`**: Removes specific records from a table without destroying the underlying table structure. *(Ex: `DELETE FROM Customers WHERE ID = 1;`)*

### 3. DQL (Data Query Language)

*   **Term Definition:** DQL is a highly specialized subset of SQL (and arguably the most frequently executed) dedicated exclusively to the retrieval, computation, and projection of data from the database without mutating the underlying data's state.
*   **How it works:** DQL operates on a declarative programming model. The user merely specifies *what result is desired*, rather than dictating the procedural steps on *how to retrieve it*. The RDBMS's Query Optimizer parses the syntax, constructs the most computationally efficient Execution Plan, reads data from disk into RAM, performs relational algebra (like JOINs), applies predicates (WHERE), and performs aggregations (GROUP BY). Ultimately, it projects a virtual Result Set back to the user. This is an idempotent operation, producing zero side-effects on the stored data.
*   **Why use it:** To extract meaningful, actionable information from massive repositories of raw data, acting as the foundation for data analysis, reporting, and business intelligence.
*   **Where to use:** Rendering a catalog of products on a graphical user interface, generating complex month-end financial aggregations (OLAP), or filtering patient medical records based on specific diagnostic criteria.
*   **Representative Commands & Examples:**
    *   **`SELECT`**: Retrieves data that satisfies predefined relational conditions. *(Ex: `SELECT Name, Email FROM Customers WHERE ID = 1;`)*
---
# QUERY DATA

---
# REFERENCE: https://www.youtube.com/watch?v=SSKVgrwhzus&list=PLNcg_FV9n7qZY_2eAtUzEUulNjTJREhQe

