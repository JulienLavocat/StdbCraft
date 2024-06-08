use log;
use spacetimedb::{query, ReducerContext, spacetimedb};

use crate::tables::{Block, BlockChange, WorldInfos};
use crate::utils::BlockBuilder;

mod tables;
mod utils;

#[spacetimedb(init)]
pub fn init() {
    log::info!("Database initialisation");
    WorldInfos::insert(WorldInfos {
        seed: 123456,
    });


    // textures: 0, stone; 1: dirt; 2: grass_side; 3: grass_top, 4: sand, 5: glass
    Block::insert(BlockBuilder::new().transparent().build()).unwrap(); // air
    Block::insert(BlockBuilder::new().all(0).build()).unwrap(); // stone
    Block::insert(BlockBuilder::new().all(1).build()).unwrap(); // dirt
    Block::insert(BlockBuilder::new().sides(2).top(3).bottom(1).build()).unwrap(); // grass
    Block::insert(BlockBuilder::new().all(4).build()).unwrap(); // sand
    Block::insert(BlockBuilder::new().all(5).transparent().build()).unwrap(); // glass
}

#[spacetimedb(connect)]
pub fn identity_connected(ctx: ReducerContext) {
    log::info!("[Identity#{}] connected", ctx.sender.to_abbreviated_hex());
    log::info!("Block changes {}", BlockChange::iter().count())
}

#[spacetimedb(disconnect)]
pub fn identity_disconnected(ctx: ReducerContext) {
    log::info!("[Identity#{}] disconnected", ctx.sender.to_abbreviated_hex())
}

#[spacetimedb(reducer)]
pub fn send_block_change(_ctx: ReducerContext, x: i32, y: i32, z: i32, block_id: i32) -> Result<(), String> {
    for mut bc in query!(|bc: BlockChange| bc.x == x && bc.y == y && bc.z == z) {
        bc.block_id = block_id;
        let id = bc.id;
        BlockChange::update_by_id(&id, bc);
        log::info!("BlockChange updated ({}, {}, {}) -> {}", x, y, z, block_id);
        return Ok(());
    }

    BlockChange::insert(BlockChange {
        id: 0,
        x,
        y,
        z,
        block_id,
    }).expect("Unable to save block change");

    log::info!("BlockChange created ({}, {}, {}) -> {}", x, y, z, block_id);
    Ok(())
}
