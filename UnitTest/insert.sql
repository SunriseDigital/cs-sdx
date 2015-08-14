INSERT INTO category_type (name, code) VALUES ('飲食店', 'restaurant');
INSERT INTO category_type (name, code) VALUES ('美容室', 'salon');

INSERT INTO category (name, code, category_type_id) VALUES ('中華', 'chinese', (SELECT id FROM category_type WHERE code ='restaurant'));
INSERT INTO category (name, code, category_type_id) VALUES ('イタリアン', 'italian', (SELECT id FROM category_type WHERE code ='restaurant'));
INSERT INTO category (name, code, category_type_id) VALUES ('美容室', 'beauty_salon', (SELECT id FROM category_type WHERE code ='salon'));
INSERT INTO category (name, code, category_type_id) VALUES ('ネイルサロン', 'nail_salon', (SELECT id FROM category_type WHERE code ='salon'));

INSERT INTO shop (name, category_id) VALUES ('天祥', (SELECT id FROM category WHERE code = 'chinese'));
INSERT INTO shop (name, category_id) VALUES ('エスペリア', (SELECT id FROM category WHERE code = 'italian'));
INSERT INTO shop (name, category_id) VALUES ('天府舫', (SELECT id FROM category WHERE code = 'chinese'));
INSERT INTO shop (name, category_id) VALUES ('Aveda', (SELECT id FROM category WHERE code = 'beauty_salon'));
INSERT INTO shop (name, category_id) VALUES ('ビーナスラッシュ', (SELECT id FROM category WHERE code = 'nail_salon'));

INSERT INTO menu (name, shop_id) VALUES ('干し豆腐のサラダ', (SELECT id FROM shop WHERE name = '天府舫'));
INSERT INTO menu (name, shop_id) VALUES ('麻婆豆腐', (SELECT id FROM shop WHERE name = '天府舫'));
INSERT INTO menu (name, shop_id) VALUES ('牛肉の激辛水煮', (SELECT id FROM shop WHERE name = '天府舫'));
