<?php
$host = 'mysql-db';
$user = 'user';           
//$user = 'root';           
$pass = 'resu';
//$pass = 'toor';
$db   = 'database_webadv';
$charset = 'utf8mb4';
$options = [
    PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
    PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC
];
?>
<pre>
<?php
try {
    $dsn = "mysql:host=$host;charset=$charset";
    print("dsn(data source name)=$dsn\n");
    print("sql-query = SHOW DATABASES\n");
    $pdo = new PDO($dsn, $user, $pass, $options);
    $statement = $pdo->query( "SHOW DATABASES\n" );
    while( $row = $statement->fetch() ) {
        print( '- '.implode(', ', $row)."\n" );
    } 
    print("done");
} catch (Exception $e) {
     var_dump($e);
}
?>
</pre>
<hr>
<pre>
<?php
$sqlQuery = "
DROP DATABASE IF EXISTS $db;
CREATE DATABASE $db;

USE $db;

CREATE TABLE person (
  id INT UNSIGNED NOT NULL AUTO_INCREMENT,
  name VARCHAR(255) NOT NULL,
  PRIMARY KEY (id)
);

INSERT INTO person (id, name) VALUES
  (1, 'Alice'),
  (2, 'Bob'),
  (3, 'Charlie'); 
";

try {
    $dsn = "mysql:host=$host;charset=$charset";
    print("dsn(data source name)=$dsn\n");
    print("sql command=$sqlQuery\n");
    $pdo = new PDO($dsn, $user, $pass, $options);
    $result = $pdo->exec($sqlQuery);
    print("done");    
} catch (Exception $e) {
    var_dump($e);
}
?>
</pre>
<hr>
<pre>
<?php
try {
    $dsn = "mysql:host=$host;dbname=$db;charset=$charset";
    print("dsn(data source name)=$dsn\n");
    print("sql command = SELECT * FROM person\n");
    $pdo = new PDO($dsn, $user, $pass, $options);
    $statement = $pdo->query('SELECT * FROM person');   
     while( $row = $statement->fetch() ) {
            print( '- '.implode(', ', $row)."\n" );
        } 
    print("done");        
} catch (Exception $e) {
    var_dump($e);
}
?>
</pre>




