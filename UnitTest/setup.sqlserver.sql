CREATE TABLE shop (
  id int IDENTITY ,
  name nvarchar(100),
  category_id int NOT NULL,
  CONSTRAINT pk_shop PRIMARY KEY CLUSTERED (id)
);

CREATE TABLE category (
  id int IDENTITY ,
  name nvarchar(100),
  code nvarchar(50),
  CONSTRAINT pk_category PRIMARY KEY CLUSTERED (id),
  CONSTRAINT key_category_code UNIQUE NONCLUSTERED (code)
);

ALTER TABLE shop ADD CONSTRAINT fk_shop_category_id
　FOREIGN KEY (category_id)
　REFERENCES category(id);
