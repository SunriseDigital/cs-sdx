INSERT INTO large_area (name, code) VALUES ('東京', 'tokyo');
INSERT INTO large_area (name, code) VALUES ('愛知', 'aichi');

INSERT INTO area (name, code, large_area_id) VALUES ('新宿', 'sinjuku', (SELECT id FROM large_area WHERE code = 'tokyo'));
INSERT INTO area (name, code, large_area_id) VALUES ('新中野', 'sinnakano', (SELECT id FROM large_area WHERE code = 'tokyo'));
INSERT INTO area (name, code, large_area_id) VALUES ('西麻布', 'nisiazabu', (SELECT id FROM large_area WHERE code = 'tokyo'));
INSERT INTO area (name, code, large_area_id) VALUES ('渋谷', 'sibuya', (SELECT id FROM large_area WHERE code = 'tokyo'));
INSERT INTO area (name, code, large_area_id) VALUES ('矢場町', 'yabacho', (SELECT id FROM large_area WHERE code = 'aichi'));


INSERT INTO shop (name, area_id) VALUES ('天祥', (SELECT id FROM area WHERE name = '新中野'));
INSERT INTO shop (name, area_id) VALUES ('エスペリア', (SELECT id FROM area WHERE name = '西麻布'));
INSERT INTO shop (name, area_id) VALUES ('天府舫', (SELECT id FROM area WHERE name = '新宿'));
INSERT INTO shop (name, area_id) VALUES ('Freeve', (SELECT id FROM area WHERE name = '渋谷'));
INSERT INTO shop (name, area_id) VALUES ('ビーナスラッシュ', (SELECT id FROM area WHERE name = '渋谷'));
INSERT INTO shop (name, area_id) VALUES ('味仙', (SELECT id FROM area WHERE name = '矢場町'));

INSERT INTO category (name, code) VALUES ('中華', 'chinese');
INSERT INTO category (name, code) VALUES ('イタリアン', 'italian');
INSERT INTO category (name, code) VALUES ('美容室', 'beauty_salon');
INSERT INTO category (name, code) VALUES ('ネイルサロン', 'nail_salon');
INSERT INTO category (name, code) VALUES ('まつげエクステ', 'eyelash_salon');

INSERT INTO shop_category (shop_id, category_id) VALUES ((SELECT id FROM shop WHERE name = 'Freeve'), (SELECT id FROM category WHERE name = 'ネイルサロン'));
INSERT INTO shop_category (shop_id, category_id) VALUES ((SELECT id FROM shop WHERE name = 'Freeve'), (SELECT id FROM category WHERE name = '美容室'));
INSERT INTO shop_category (shop_id, category_id) VALUES ((SELECT id FROM shop WHERE name = 'Freeve'), (SELECT id FROM category WHERE name = 'まつげエクステ'));

INSERT INTO shop_category (shop_id, category_id) VALUES ((SELECT id FROM shop WHERE name = 'ビーナスラッシュ'), (SELECT id FROM category WHERE name = 'ネイルサロン'));
INSERT INTO shop_category (shop_id, category_id) VALUES ((SELECT id FROM shop WHERE name = 'ビーナスラッシュ'), (SELECT id FROM category WHERE name = 'まつげエクステ'));

INSERT INTO menu (name, shop_id) VALUES ('干し豆腐のサラダ', (SELECT id FROM shop WHERE name = '天府舫'));
INSERT INTO menu (name, shop_id) VALUES ('麻婆豆腐', (SELECT id FROM shop WHERE name = '天府舫'));
INSERT INTO menu (name, shop_id) VALUES ('牛肉の激辛水煮', (SELECT id FROM shop WHERE name = '天府舫'));

