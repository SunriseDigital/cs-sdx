INSERT INTO category_type (name, code) VALUES ('飲食店', 'restaurant');

INSERT INTO category (name, code, category_type_id) VALUES ('中華', 'chinese', (SELECT id FROM category_type WHERE code ='restaurant'));
INSERT INTO category (name, code, category_type_id) VALUES ('イタリアン', 'italian', (SELECT id FROM category_type WHERE code ='restaurant'));

INSERT INTO shop (name, category_id) VALUES ('天祥', (SELECT id FROM category WHERE code = 'chinese'));
INSERT INTO shop (name, category_id) VALUES ('エスペリア', (SELECT id FROM category WHERE code = 'italian'));
