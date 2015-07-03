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
  category_type_id int NOT NULL,
  PRIMARY KEY (id),
  UNIQUE INDEX key_category_code (code ASC)
);

ALTER TABLE shop ADD FOREIGN KEY (category_id) REFERENCES category(id);

CREATE TABLE category_type (
  id int NOT NULL AUTO_INCREMENT ,
  name varchar(100),
  code varchar(50),
  PRIMARY KEY (id),
  UNIQUE INDEX key_category_type_code (code ASC)
);

ALTER TABLE category ADD FOREIGN KEY (category_type_id) REFERENCES category_type(id);
