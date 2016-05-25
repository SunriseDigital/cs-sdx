CREATE TABLE shop (
  id int IDENTITY ,
  name nvarchar(100),
  area_id int NOT NULL,
  main_image_id int,
  sub_image_id int,
  login_id nvarchar(190),
  password nvarchar(190),
  created_at datetime NOT NULL,
  CONSTRAINT pk_shop PRIMARY KEY CLUSTERED (id)
);

CREATE TABLE shop_image (
  shop_id int,
  shop_image_type_id int,
  path nvarchar(255) NOT NULL,
  created_at datetime NOT NULL,
  CONSTRAINT pk_shop_image PRIMARY KEY (shop_id, shop_image_type_id)
);

CREATE TABLE shop_image_type (
  id int IDENTITY ,
  name nvarchar(50),
  CONSTRAINT pk_shop_image_type PRIMARY KEY CLUSTERED (id)
);

ALTER TABLE shop_image ADD CONSTRAINT fk_shop_image_shop_id
　FOREIGN KEY (shop_id)
　REFERENCES shop(id);

ALTER TABLE shop_image ADD CONSTRAINT fk_shop_image_shop_image_type_id
　FOREIGN KEY (shop_image_type_id)
　REFERENCES shop_image_type(id);

CREATE TABLE area (
  id int IDENTITY ,
  name nvarchar(50),
  large_area_id int,
  code nvarchar(50) NOT NULL,
  sequence int NOT NULL DEFAULT 0,
  CONSTRAINT pk_area PRIMARY KEY CLUSTERED (id),
  CONSTRAINT key_area_code UNIQUE NONCLUSTERED (code)
);

CREATE TABLE large_area (
  id int IDENTITY ,
  name nvarchar(100),
  code nvarchar(50) NOT NULL,
  CONSTRAINT pk_large_area PRIMARY KEY CLUSTERED (id),
  CONSTRAINT key_large_area_code UNIQUE NONCLUSTERED (code)
);

ALTER TABLE area ADD CONSTRAINT fk_area_large_area_id
　FOREIGN KEY (large_area_id)
　REFERENCES large_area(id);

ALTER TABLE shop ADD CONSTRAINT fk_shop_area_id
　FOREIGN KEY (area_id)
　REFERENCES area(id);


CREATE TABLE image (
  id int IDENTITY ,
  path nvarchar(190),
  CONSTRAINT pk_image PRIMARY KEY CLUSTERED (id)
);

ALTER TABLE shop ADD CONSTRAINT fk_shop_main_image_id
　FOREIGN KEY (main_image_id)
　REFERENCES image(id);

ALTER TABLE shop ADD CONSTRAINT fk_shop_sub_image_id
　FOREIGN KEY (sub_image_id)
　REFERENCES image(id);

 -- OneMany
CREATE TABLE menu (
  id int IDENTITY ,
  name nvarchar(50),
  shop_id int,
  CONSTRAINT pk_menu PRIMARY KEY CLUSTERED (id)
);

ALTER TABLE menu ADD CONSTRAINT fk_menu_shop_id
　FOREIGN KEY (shop_id)
　REFERENCES shop(id);

 -- ManyMany

 CREATE TABLE category (
  id int IDENTITY ,
  name nvarchar(100),
  code nvarchar(50),
  CONSTRAINT pk_category PRIMARY KEY CLUSTERED (id),
  CONSTRAINT key_category_code UNIQUE NONCLUSTERED (code)
);

CREATE TABLE shop_category (
  shop_id int NOT NULL,
  category_id int NOT NULL,
  PRIMARY KEY (shop_id, category_id)
);

ALTER TABLE shop_category ADD CONSTRAINT fk_shop_category_shop_id
　FOREIGN KEY (shop_id)
　REFERENCES shop(id)
  ON DELETE CASCADE;

ALTER TABLE shop_category ADD CONSTRAINT fk_shop_category_category_id
　FOREIGN KEY (category_id)
　REFERENCES area(id);