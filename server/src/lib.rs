mod tables;

use spacetimedb::{spacetimedb, ReducerContext};
use log;
use crate::tables::{Block, WorldInfos};

#[spacetimedb(init)]
pub fn init() {
    log::info!("Database initialisation");
    WorldInfos::insert(WorldInfos{
        seed: 123456
    });

    // textures: 0, stone; 1: dirt; 2: grass_side; 3: grass_top, 4: sand, 5: glass
    Block::insert(Block{is_transparent: true, ..Default::default()}).unwrap(); // air
    Block::insert(Block{side: 0, ..Default::default()}).unwrap(); // stone
    Block::insert(Block{side: 1, ..Default::default()}).unwrap(); // dirt
    Block::insert(Block{top: 3, bottom: 1, side: 2, ..Default::default()}).unwrap(); // grass
    Block::insert(Block{side: 4, ..Default::default()}).unwrap(); // sand
    Block::insert(Block{side: 5, is_transparent: true, ..Default::default()}).unwrap(); // glass
}

#[spacetimedb(connect)]
pub fn identity_connected(ctx: ReducerContext) {
    log::info!("[Identity#{}] connected", ctx.sender.to_abbreviated_hex())
}

#[spacetimedb(disconnect)]
pub fn identity_disconnected(ctx: ReducerContext) {
    log::info!("[Identity#{}] disconnected", ctx.sender.to_abbreviated_hex())
}
