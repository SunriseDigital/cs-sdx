﻿CREATE TABLE shop (
  id int NOT NULL AUTO_INCREMENT,
  name varchar(100) NOT NULL,
  area_id int NOT NULL,
  main_image_id int,
  sub_image_id int,
  login_id varchar(190),
  password varchar(190),
  created_at datetime NOT NULL,
  PRIMARY KEY (id)
);

CREATE TABLE shop_image (
  shop_id int,
  shop_image_type_id int,
  path varchar(255) NOT NULL,
  created_at datetime NOT NULL,
  PRIMARY KEY(shop_id, shop_image_type_id)
);

CREATE TABLE shop_image_type (
  id int NOT NULL AUTO_INCREMENT,
  name varchar(50),
  PRIMARY KEY (id)
);

ALTER TABLE shop_image ADD FOREIGN KEY (shop_id) REFERENCES shop(id);
ALTER TABLE shop_image ADD FOREIGN KEY (shop_image_type_id) REFERENCES shop_image_type(id);

CREATE TABLE area (
  id int NOT NULL AUTO_INCREMENT,
  name varchar(50),
  large_area_id int,
  code varchar(50) NOT NULL,
  sequence int NOT NULL DEFAULT 0,
  PRIMARY KEY (id),
  UNIQUE INDEX large_area_code (code ASC)
);

ALTER TABLE shop ADD FOREIGN KEY (area_id) REFERENCES area(id);

CREATE TABLE large_area (
  id int NOT NULL AUTO_INCREMENT ,
  name varchar(100),
  code varchar(50) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE INDEX large_area_code (code ASC)
);

ALTER TABLE area ADD FOREIGN KEY (large_area_id) REFERENCES large_area(id);

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
CREATE TABLE category (
  id int NOT NULL AUTO_INCREMENT ,
  name varchar(100),
  code varchar(50),
  PRIMARY KEY (id),
  UNIQUE INDEX key_category_code (code ASC)
);

CREATE TABLE shop_category (
  shop_id int,
  category_id int,
  PRIMARY KEY (shop_id, category_id)
);

ALTER TABLE shop_category ADD FOREIGN KEY (shop_id) REFERENCES shop(id) ON DELETE CASCADE;
ALTER TABLE shop_category ADD FOREIGN KEY (category_id) REFERENCES category(id);

CREATE TABLE get_orm (
  id int NOT NULL AUTO_INCREMENT,
  double_column DOUBLE PRECISION UNSIGNED ZEROFILL,
  text_value text CHARACTER SET utf8 COLLATE utf8_general_ci,
  size ENUM('x-small', 'small', 'medium', 'large', 'x-large'),
  varchar_value varchar(100),
  real_value REAL(1, 1),
  timestamp_value TIMESTAMP,
  PRIMARY KEY (id)
);
