CREATE TABLE shop (
  id int NOT NULL AUTO_INCREMENT,
  name varchar(100),
  category_id int NOT NULL,
  PRIMARY KEY (id)
);

CREATE TABLE category (
  id int NOT NULL AUTO_INCREMENT ,
  name varchar(100),
  code varchar(50),
  PRIMARY KEY (id),
  UNIQUE INDEX key_category_code (code ASC)
);

ALTER TABLE shop ADD FOREIGN KEY (category_id) REFERENCES category(id);
