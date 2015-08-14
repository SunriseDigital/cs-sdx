CREATE TABLE shop (
  id int NOT NULL AUTO_INCREMENT,
  name varchar(100),
  category_id int NOT NULL,
  main_image_id int,
  sub_image_id int,
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

CREATE TABLE image (
  id int NOT NULL AUTO_INCREMENT,
  path varchar(190),
  PRIMARY KEY (id)
);

ALTER TABLE shop ADD FOREIGN KEY (main_image_id) REFERENCES image(id);
ALTER TABLE shop ADD FOREIGN KEY (sub_image_id) REFERENCES image(id);

-- OneMany
CREATE TABLE menu (
  id int NOT NULL AUTO_INCREMENT,
  name varchar(50),
  shop_id int,
  PRIMARY KEY (id)
);

ALTER TABLE menu ADD FOREIGN KEY (shop_id) REFERENCES shop(id);

-- ManyMany
CREATE TABLE area (
  id int NOT NULL AUTO_INCREMENT,
  name varchar(50),
  PRIMARY KEY (id)
);

CREATE TABLE shop_area (
  shop_id int,
  area_id int,
  PRIMARY KEY (shop_id, area_id)
);

ALTER TABLE shop_area ADD FOREIGN KEY (shop_id) REFERENCES shop(id);
ALTER TABLE shop_area ADD FOREIGN KEY (area_id) REFERENCES area(id);