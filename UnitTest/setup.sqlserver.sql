CREATE TABLE shop (
  id int IDENTITY ,
  name nvarchar(100),
  category_id int NOT NULL,
  main_image_id int,
  sub_image_id int,
  CONSTRAINT pk_shop PRIMARY KEY CLUSTERED (id)
);

CREATE TABLE category (
  id int IDENTITY ,
  name nvarchar(100),
  code nvarchar(50),
  category_type_id int NOT NULL,
  CONSTRAINT pk_category PRIMARY KEY CLUSTERED (id),
  CONSTRAINT key_category_code UNIQUE NONCLUSTERED (code)
);

ALTER TABLE shop ADD CONSTRAINT fk_shop_category_id
　FOREIGN KEY (category_id)
　REFERENCES category(id);



CREATE TABLE category_type (
  id int IDENTITY ,
  name nvarchar(100),
  code nvarchar(50),
  CONSTRAINT pk_category_type PRIMARY KEY CLUSTERED (id),
  CONSTRAINT key_category_type_code UNIQUE NONCLUSTERED (code)
);

ALTER TABLE category ADD CONSTRAINT fk_category_category_type_id
　FOREIGN KEY (category_type_id)
　REFERENCES category_type(id);

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