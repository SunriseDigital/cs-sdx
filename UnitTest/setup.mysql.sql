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

INSERT INTO category (name, code) VALUES ('中華', 'chinese');
INSERT INTO category (name, code) VALUES ('イタリアン', 'italian');

INSERT INTO shop (name, category_id) VALUES ('天祥', (SELECT id FROM category WHERE code = 'chinese'));
INSERT INTO shop (name, category_id) VALUES ('エスペリア', (SELECT id FROM category WHERE code = 'italian'));